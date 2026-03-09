using System.Text;
using System.Text.Json;
using Anthropic.Models.Messages;
using PayOS.Exceptions;
using TechExpress.Repository;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;

namespace TechExpress.Service.Services;

public class ChatService(UnitOfWork unitOfWork, ChatAiService chatAiService)
{
    private readonly UnitOfWork _unitOfWork = unitOfWork;
    private readonly ChatAiService _chatAiService = chatAiService;

    public async Task<ChatSession> HandleCreateSession(string? userIdStr, string? fullName, string? phone)
    {
        ChatSession? unclosedSession;
        Guid? userId;
        if (userIdStr is not null)
        {
            userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ?? throw new NotFoundException($"Không tìm thấy người dùng: {userId}");
            unclosedSession = await _unitOfWork.ChatSessionRepository.FindByUserIdAndNotClosedAsync(userId.Value);
            fullName = user.FirstName + user.LastName;
            phone = null;
        }
        else if (fullName is not null && phone is not null)
        {
            unclosedSession = await _unitOfWork.ChatSessionRepository.FindByPhoneAndNotClosedAsync(phone);
            userId = null;
        }
        else
        {
            throw new BadRequestException("Tên đầy đủ và số điện thoại là bắt buộc để thực hiện tạo phiên hội thoại");
        }
        if (unclosedSession is null)
        {
            unclosedSession = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = fullName,
                Phone = phone
            };
            await _unitOfWork.ChatSessionRepository.AddAsync(unclosedSession);
            await _unitOfWork.SaveChangesAsync();
        }

        var session = await _unitOfWork.ChatSessionRepository.FindByIdAsync(unclosedSession.Id) ?? throw new NotFoundException($"Không tìm thấy phiên trò chuyện {unclosedSession.Id}");
        return session;
    }

    public async Task<(List<ChatMessage>, int, bool)> HandleLoadMessages(Guid sessionId, string? userIdStr, string? phone, int size, int pageIndex)
    {
        var session = await _unitOfWork.ChatSessionRepository.FindByIdAsync(sessionId) ?? throw new NotFoundException($"Không tìm thấy phiên trò chuyện {sessionId}");
        Guid? userId;
        if (userIdStr is not null)
        {
            userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ?? throw new NotFoundException($"Không tìm thấy người dùng {userId}");
            if (user.IsCustomerUser() && session.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện này");
            }
        }
        else if (phone is not null)
        {
            if (session.Phone != phone)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện");
            }
        }
        else
        {
            throw new BadRequestException("Đăng nhập hoặc yêu cầu thông qua số điện thoại để truy cập vào cuộc trò chuyện");
        }
        var messages = await _unitOfWork.ChatMessageRepository.FindChatMessagesIncludeMediasBySessionIdWithSplitQueryAsync(sessionId, size, pageIndex);
        bool isMore = messages.Count > size;
        if (isMore)
        {
            messages.RemoveAt(messages.Count - 1);
        }
        messages.Reverse();
        return (messages, pageIndex, isMore);
    }

    public async Task<(ChatMessage CustomerMsg, bool ShouldTriggerAi)> HandleSendMessage(Guid sessionId, string? userIdStr, string? phone, string message, List<(string, ChatMediaType)> medias)
    {
        var session = await _unitOfWork.ChatSessionRepository.FindByIdAsync(sessionId) ?? throw new NotFoundException($"Không tìm thấy phiên trò chuyện {sessionId}");
        Guid? userId;
        string? sentByFullName;
        bool isCustomerOrGuest;
        if (userIdStr is not null)
        {
            userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ?? throw new NotFoundException($"Không tìm thấy người dùng {userId}");
            if (user.IsCustomerUser() && session.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện này");
            }
            isCustomerOrGuest = user.IsCustomerUser();
            sentByFullName = user.IsCustomerUser() ? user.FirstName + user.LastName : "Nhân viên hỗ trợ";
        }
        else if (phone is not null)
        {
            if (session.Phone != phone)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện");
            }
            sentByFullName = session.FullName;
            userId = null;
            isCustomerOrGuest = true;
        }
        else
        {
            throw new BadRequestException("Đăng nhập hoặc yêu cầu thông qua số điện thoại để truy cập vào cuộc trò chuyện");
        }
        if (medias.Count > 5)
        {
            throw new BadRequestException("Chỉ hỗ trợ tối đa 5 file phương tiện");
        }
        var messageId = Guid.NewGuid();
        var msg = new ChatMessage
        {
            Id = messageId,
            SessionId = sessionId,
            Message = message,
            SentById = userId,
            SentByFullName = sentByFullName,
            Medias = [.. medias.Select(m => new ChatMedia
            {
                MessageId = messageId,
                MediaUrl = m.Item1,
                Type = m.Item2
            })]
        };
        await _unitOfWork.ChatMessageRepository.AddAsync(msg);
        await _unitOfWork.SaveChangesAsync();

        var newMsg = await _unitOfWork.ChatMessageRepository.FindByIdIncludeMediasAsync(msg.Id) ?? throw new NotFoundException($"Không tìm thấy tin nhắn {msg.Id}");
        bool shouldTriggerAi = isCustomerOrGuest && !session.IsEscalated;
        return (newMsg, shouldTriggerAi);
    }

    public async Task<ChatMessage?> HandleGenerateAiReply(Guid sessionId)
    {
        var history = await _unitOfWork.ChatMessageRepository.FindRecentMessagesForAiContextAsync(sessionId, 20);
        if (history.Count == 0) return null;

        var messages = _chatAiService.BuildMessagesFromHistory(history);
        string? finalReply = null;
        bool aiUnavailable = false;

        try
        {
            // Agentic loop — max 10 iterations to prevent runaway tool calls
            for (int i = 0; i < 10; i++)
            {
                var response = await _chatAiService.CallApiAsync(messages);

                if (response.StopReason == StopReason.ToolUse)
                {
                    // Build assistant turn (text + tool_use blocks)
                    var assistantBlocks = new List<ContentBlockParam>();
                    var toolResultBlocks = new List<ContentBlockParam>();

                    foreach (var block in response.Content)
                    {
                        if (block.TryPickText(out var textBlock))
                        {
                            assistantBlocks.Add(new ContentBlockParam(new TextBlockParam(textBlock.Text), null));
                        }
                        else if (block.TryPickToolUse(out var toolUseBlock))
                        {
                            assistantBlocks.Add(new ContentBlockParam(
                                new ToolUseBlockParam { ID = toolUseBlock.ID, Name = toolUseBlock.Name, Input = toolUseBlock.Input },
                                null));

                            var result = await ExecuteToolAsync(toolUseBlock.Name, sessionId, toolUseBlock.Input);
                            toolResultBlocks.Add(new ContentBlockParam(
                                new ToolResultBlockParam(toolUseBlock.ID) { Content = new ToolResultBlockParamContent(result, null) },
                                null));
                        }
                    }

                    messages.Add(new MessageParam { Role = Role.Assistant, Content = new MessageParamContent(assistantBlocks, null) });
                    messages.Add(new MessageParam { Role = Role.User, Content = new MessageParamContent(toolResultBlocks, null) });
                    continue;
                }

                // EndTurn, MaxTokens, or other — extract text and stop
                foreach (var block in response.Content)
                {
                    if (block.TryPickText(out var textBlock))
                    {
                        finalReply = textBlock.Text;
                        break;
                    }
                }
                break;
            }
        }
        catch
        {
            aiUnavailable = true;
        }

        if (aiUnavailable || string.IsNullOrEmpty(finalReply))
        {
            var session = await _unitOfWork.ChatSessionRepository.FindByIdWithTrackingAsync(sessionId);
            if (session is not null && !session.IsEscalated)
            {
                session.IsEscalated = true;
                session.UpdatedAt = DateTimeOffset.Now;
                await _unitOfWork.SaveChangesAsync();
            }
            finalReply = "Xin lỗi, trợ lý AI hiện không khả dụng. Phiên trò chuyện của bạn đã được chuyển đến nhân viên hỗ trợ, chúng tôi sẽ phản hồi sớm nhất có thể.";
        }

        var aiMessageId = Guid.NewGuid();
        var aiMessage = new ChatMessage
        {
            Id = aiMessageId,
            SessionId = sessionId,
            Message = finalReply,
            SentById = null,
            SentByFullName = "AI Assistant",
            IsAiMessage = true,
            Medias = []
        };
        await _unitOfWork.ChatMessageRepository.AddAsync(aiMessage);
        await _unitOfWork.SaveChangesAsync();
        return await _unitOfWork.ChatMessageRepository.FindByIdIncludeMediasAsync(aiMessageId);
    }

    public async Task<List<ChatSession>> HandleGetAllSessions(bool? isClosed)
    {
        List<ChatSession> sessions = [];
        if (isClosed is not null)
        {
            sessions = await _unitOfWork.ChatSessionRepository.FindByIsClosedAsync(isClosed.Value);
        }
        else
        {
            sessions = await _unitOfWork.ChatSessionRepository.FindAllAsync();
        }
        return sessions;
    }


    private async Task<string> ExecuteToolAsync(string toolName, Guid sessionId, IReadOnlyDictionary<string, JsonElement> input)
    {
        try
        {
            return toolName switch
            {
                "search_products" => await SearchProductsToolAsync(input),
                "get_product_detail" => await GetProductDetailToolAsync(input),
                "check_pc_compatibility" => await CheckPcCompatibilityToolAsync(input),
                "escalate_to_staff" => await EscalateToStaffToolAsync(sessionId),
                _ => $"Unknown tool: {toolName}"
            };
        }
        catch (Exception ex)
        {
            return $"Tool error: {ex.Message}";
        }
    }


    private async Task<string> SearchProductsToolAsync(IReadOnlyDictionary<string, JsonElement> input)
    {
        string? keywords = input.TryGetValue("keywords", out var kw) && kw.ValueKind == JsonValueKind.String ? kw.GetString() : null;
        string? categoryName = input.TryGetValue("category", out var cat) && cat.ValueKind == JsonValueKind.String ? cat.GetString() : null;
        string? brandName = input.TryGetValue("brand", out var br) && br.ValueKind == JsonValueKind.String ? br.GetString() : null;
        decimal? minPrice = input.TryGetValue("min_price", out var minEl) && minEl.ValueKind == JsonValueKind.Number ? minEl.GetDecimal() : null;
        decimal? maxPrice = input.TryGetValue("max_price", out var maxEl) && maxEl.ValueKind == JsonValueKind.Number ? maxEl.GetDecimal() : null;

        List<Guid>? categoryIds = null;
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var allCategories = await _unitOfWork.CategoryRepository.GetAllCategoriesAsync();
            var matched = allCategories
                .Where(c => c.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Id)
                .ToList();
            categoryIds = matched.Count > 0 ? matched : [Guid.Empty]; // force empty result if category not found
        }

        Guid? brandId = null;
        if (!string.IsNullOrWhiteSpace(brandName))
        {
            var (brands, _) = await _unitOfWork.BrandRepository.GetPagedAsync(1, 10, brandName, null, null);
            brandId = brands.FirstOrDefault()?.Id;
        }

        var (products, totalCount) = await _unitOfWork.ProductRepository.FindProductsPagedSortByPriceAsync(
            page: 1,
            pageSize: 6,
            isDescending: false,
            search: keywords,
            categoryIds: categoryIds,
            status: ProductStatus.Available,
            brandId: brandId);

        // Apply client-side price filter if needed (repository doesn't have price range filter)
        if (minPrice.HasValue) products = [.. products.Where(p => p.Price >= minPrice.Value)];
        if (maxPrice.HasValue) products = [.. products.Where(p => p.Price <= maxPrice.Value)];

        if (products.Count == 0)
            return "No products found matching the search criteria.";

        var sb = new StringBuilder();
        sb.AppendLine($"Found {Math.Min(products.Count, 5)} of {totalCount} matching products:");
        foreach (var p in products.Take(5))
        {
            sb.AppendLine($"- [{p.Id}] {p.Name}");
            sb.AppendLine($"  Category: {p.Category?.Name ?? "N/A"} | Price: {p.Price:N0} VND | Stock: {p.Stock}");
        }
        return sb.ToString().TrimEnd();
    }



    private async Task<string> GetProductDetailToolAsync(IReadOnlyDictionary<string, JsonElement> input)
    {
        if (!input.TryGetValue("product_id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
            return "Missing required parameter: product_id";
        if (!Guid.TryParse(idEl.GetString(), out var productId))
            return "Invalid product_id format — must be a UUID";

        var product = await _unitOfWork.ProductRepository
            .FindByIdIncludeCategoryAndImagesAndSpecValuesThenIncludeSpecDefinitionWithSplitQueryAsync(productId);

        if (product is null)
            return $"Product with ID {productId} not found.";

        var sb = new StringBuilder();
        sb.AppendLine($"[{product.Id}] {product.Name}");
        sb.AppendLine($"Category: {product.Category?.Name ?? "N/A"} | Price: {product.Price:N0} VND | Stock: {product.Stock} | Warranty: {product.WarrantyMonth}mo");

        if (product.SpecValues?.Count > 0)
        {
            // Limit to 10 most relevant specs to keep token usage low
            sb.AppendLine("Specs:");
            foreach (var sv in product.SpecValues.Take(10))
            {
                var specName = sv.SpecDefinition?.Name ?? sv.SpecDefinitionId.ToString();
                var unit = sv.SpecDefinition?.Unit;
                var value = sv.TextValue ?? sv.NumberValue?.ToString() ?? sv.DecimalValue?.ToString() ?? sv.BoolValue?.ToString() ?? "N/A";
                sb.AppendLine($"  {specName}: {value}{(unit != null ? " " + unit : "")}");
            }
        }

        return sb.ToString().TrimEnd();
    }



    private async Task<string> CheckPcCompatibilityToolAsync(IReadOnlyDictionary<string, JsonElement> input)
    {
        if (!input.TryGetValue("components", out var componentsEl) || componentsEl.ValueKind != JsonValueKind.Array)
            return "Missing required parameter: components (must be an array)";

        var commands = new List<AddComputerComponentCommand>();
        foreach (var item in componentsEl.EnumerateArray())
        {
            if (!item.TryGetProperty("product_id", out var pidEl) || pidEl.ValueKind != JsonValueKind.String)
                continue;
            if (!Guid.TryParse(pidEl.GetString(), out var pid))
                continue;
            int qty = 1;
            if (item.TryGetProperty("quantity", out var qtyEl) && qtyEl.ValueKind == JsonValueKind.Number)
                qty = qtyEl.GetInt32();
            commands.Add(new AddComputerComponentCommand { ComponentId = pid, Quantity = qty });
        }

        if (commands.Count == 0)
            return "No valid components provided.";

        var compatibilityService = new ComputerCompatibilityService(_unitOfWork);
        var productIds = commands.Select(c => c.ComponentId).ToList();

        List<Product> products;
        try
        {
            products = await compatibilityService.GetComponentProductsFromRequestedIds(productIds);
        }
        catch (Exception ex)
        {
            return $"Product lookup failed: {ex.Message}";
        }

        List<string> warnings;
        try
        {
            warnings = await compatibilityService.CheckComputerCompatibility(commands, products);
        }
        catch (Exception ex)
        {
            return $"Compatibility issue: {ex.Message}";
        }

        if (warnings.Count == 0)
            return "All components are compatible with each other.";

        var sb = new StringBuilder();
        sb.AppendLine("Components are compatible but with performance warnings:");
        foreach (var w in warnings)
            sb.AppendLine($"  - {w}");
        return sb.ToString().TrimEnd();
    }

    private async Task<string> EscalateToStaffToolAsync(Guid sessionId)
    {
        var session = await _unitOfWork.ChatSessionRepository.FindByIdWithTrackingAsync(sessionId);
        if (session is null)
        {
            return $"Session with ID {sessionId} not found.";
        }
        session.IsEscalated = true;
        session.UpdatedAt = DateTimeOffset.Now;
        await _unitOfWork.SaveChangesAsync();
        return "Session escalated successfully. The session will be response back by a staff member";
    }
}
