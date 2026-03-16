using System.Net.Http.Headers;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;
using TechExpress.Service.Dtos;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services;

public class PromotionService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly NotificationHelper _notificationHelper;

    public PromotionService(UnitOfWork unitOfWork, NotificationHelper notificationHelper)
    {
        _unitOfWork = unitOfWork;
        _notificationHelper = notificationHelper;
    }

    public async Task<Promotion> HandleCreatePromotion(
        string name,
        string? code,
        string description,
        PromotionType type,
        PromotionScope scope,
        decimal? discountValue,
        decimal? maxDiscountValue,
        decimal? minOrderValue,
        List<CreatePromotionRequiredProductCommand> requiredProductCommands,
        PromotionRequiredProductLogic? requiredProductLogic,
        List<CreatePromotionFreeProductCommand> freeProductCommands,
        int? freeItemPickCount,
        Guid? categoryId,
        Guid? brandId,
        List<Guid> appliedProductIds,
        int? minAppliedQuantity,
        int? maxUsageCount,
        int? maxUsagePerUser,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        bool isStackable)
    {
        code = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpper();
        if (!string.IsNullOrWhiteSpace(code) && await _unitOfWork.PromotionRepository.ExistsByCodeAsync(code))
        {
            throw new BadRequestException($"Mã khuyến mãi đã tồn tại trong hệ thống: {code}");
        }
        ValidatePromotionRequestedStartDateAndEndDate(startDate, endDate);
        if (maxUsageCount.HasValue && maxUsagePerUser.HasValue && maxUsagePerUser.Value > maxUsageCount.Value)
        {
            throw new BadRequestException($"Giới hạn sử dụng mỗi người dùng: {maxUsagePerUser} không được phép vượt quá tổng giới hạn sử dụng: {maxUsageCount}");
        }

        Promotion promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = description,
            Type = type,
            Scope = scope,
            StartDate = startDate,
            EndDate = endDate,
            MaxUsageCount = maxUsageCount,
            MaxUsagePerUser = maxUsagePerUser,
            IsStackable = isStackable
        };

        await ValidatePromotionValuesFromRequestedType(type, discountValue, maxDiscountValue, freeItemPickCount, freeProductCommands, promotion);
        await ValidatePromotionWithRequestedScope(scope, categoryId, brandId, appliedProductIds, minAppliedQuantity, minOrderValue, promotion);
        await ValidatePromotionRequiredProductsFromRequest(requiredProductCommands, requiredProductLogic, promotion);

        await _unitOfWork.PromotionRepository.AddAsync(promotion);
        await _unitOfWork.SaveChangesAsync();

        // Gửi thông báo khuyến mãi cho tất cả khách hàng
        await _notificationHelper.CreatePromotionNotificationForAllCustomersAsync(
            promotion.Id,
            promotion.Code ?? string.Empty,
            promotion.Name);

        Promotion newPromotion = await _unitOfWork.PromotionRepository.FindByIdIncludeRequiredProductsIncludeFreeProductsIncludeAppliedProductsWithSplitQueryAsync(promotion.Id) ?? throw new NotFoundException($"Không tìm thấy khuyến mãi {promotion.Id}");
        return newPromotion;
    }

    private static void ValidatePromotionRequestedStartDateAndEndDate(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (startDate.CompareTo(endDate) >= 0)
        {
            throw new BadRequestException($"Thời gian bắt đầu khuyến mãi {startDate} không được phép ở sau hoặc trùng với ngày kết thúc khuyến mãi: {endDate}");
        }
    }

    private async Task ValidatePromotionValuesFromRequestedType(PromotionType type, decimal? discountValue, decimal? maxDiscountValue, int? freeItemPickCount, List<CreatePromotionFreeProductCommand> freeProductCommands, Promotion promotion)
    {
        switch (type)
        {
            case PromotionType.PercentageDiscount:
                ValidatePromotionValueWithPercentageDiscountType(discountValue);
                freeItemPickCount = null;
                break;
            case PromotionType.FixedDiscount:
                ValidatePromotionValueWithFixedDiscountType(discountValue);
                freeItemPickCount = null;
                maxDiscountValue = null;
                break;
            case PromotionType.FreeItem:
                await ValidateFreeItemFromRequest(freeProductCommands, freeItemPickCount, promotion);
                discountValue = null;
                maxDiscountValue = null;
                break;
            case PromotionType.FixedPrice:
                ValidatePromotionValueWithFixedPriceType(discountValue);
                freeItemPickCount = null;
                maxDiscountValue = null;
                break;
            default:
                throw new BadRequestException($"Khuyến mãi hiện tại không hỗ trợ loại {type}");
        }
        promotion.DiscountValue = discountValue;
        promotion.MaxDiscountValue = maxDiscountValue;
        promotion.FreeItemPickCount = freeItemPickCount;
    }

    private static void ValidatePromotionValueWithPercentageDiscountType(decimal? discountValue)
    {
        if (!discountValue.HasValue)
        {
            throw new BadRequestException($"Khuyến mãi kiểu giảm theo % yêu cầu giá trị được giảm");
        }
        if (discountValue.Value >= 100)
        {
            throw new BadRequestException($"Giá trị giảm của khuyến mãi % không được phép vượt quá hoặc bằng 100");
        }
    }

    private static void ValidatePromotionValueWithFixedDiscountType(decimal? discountValue)
    {
        if (!discountValue.HasValue)
        {
            throw new BadRequestException($"Khuyến mãi kiểu giảm theo giá cứng yêu cầu giá trị được giảm");
        }
    }

    private async Task ValidateFreeItemFromRequest(List<CreatePromotionFreeProductCommand> freeProductCommands, int? freeItemPickCount, Promotion promotion)
    {
        if (freeProductCommands.Count == 0)
        {
            throw new BadRequestException($"Khuyến mãi kiểu tặng sản phẩm cần phải được liệt kê danh sách sản phẩm tặng");
        }
        if (freeItemPickCount.HasValue && freeItemPickCount.Value > freeProductCommands.Count)
        {
            throw new BadRequestException($"Số lượng sản phẩm được chọn ({freeItemPickCount}) không được phép vượt quá số lượng sản phẩm tặng trong danh sách ({freeProductCommands.Count})");
        }
        List<Guid> requestedFreeItemIds = freeProductCommands.Select(p => p.ProductId).ToList();
        List<Product> freeProducts = await _unitOfWork.ProductRepository.FindByIdsAndActiveAsync(requestedFreeItemIds);
        List<Guid> missingItemIds = requestedFreeItemIds.Except(freeProducts.Select(p => p.Id)).ToList();
        if (missingItemIds.Count > 0)
        {
            string missingIds = string.Join(", ", missingItemIds);
            throw new NotFoundException($"Các sản phẩm: {missingIds} không tìm thấy hoặc không còn kinh doanh để làm sản phẩm quà tặng");
        }
        foreach (var command in freeProductCommands)
        {
            promotion.FreeProducts.Add(new PromotionFreeProduct
            {
                ProductId = command.ProductId,
                Quantity = command.Quantity
            });
        }
    }

    private static void ValidatePromotionValueWithFixedPriceType(decimal? discountValue)
    {
        if (!discountValue.HasValue)
        {
            throw new BadRequestException($"Khuyến mãi kiểu đồng giá yêu cầu giá tiền của sản phẩm");
        }
    }

    private async Task ValidatePromotionWithRequestedScope(PromotionScope scope, Guid? categoryId, Guid? brandId, List<Guid> appliedProductIds, int? minAppliedQuantity, decimal? minOrderValue, Promotion promotion)
    {
        switch (scope)
        {
            case PromotionScope.Order:
                categoryId = null;
                brandId = null;
                minAppliedQuantity = null;
                break;
            case PromotionScope.Product:
                await ValidatePromotionProductScopeFromRequest(appliedProductIds, promotion);
                minOrderValue = null;
                categoryId = null;
                brandId = null;
                break;
            case PromotionScope.Category:
                await ValidatePromotionCategoryScopeFromRequest(categoryId);
                minOrderValue = null;
                brandId = null;
                break;
            case PromotionScope.Brand:
                await ValidatePromotionBrandScopeFromRequest(brandId);
                minOrderValue = null;
                categoryId = null;
                break;
            default:
                throw new BadRequestException($"Khuyến mãi hiện tại không hỗ trợ scope {scope}");
        }
        promotion.MinOrderValue = minOrderValue;
        promotion.CategoryId = categoryId;
        promotion.BrandId = brandId;
        promotion.MinAppliedQuantity = minAppliedQuantity;
    }


    private async Task ValidatePromotionProductScopeFromRequest(List<Guid> appliedProductIds, Promotion promotion)
    {
        if (appliedProductIds.Count == 0)
        {
            throw new BadRequestException("Các sản phẩm áp dụng khuyến mãi không được để trống cho scope sản phẩm");
        }
        appliedProductIds = appliedProductIds.Distinct().ToList();
        List<Product> appliedProducts = await _unitOfWork.ProductRepository.FindByIdsAndActiveAsync(appliedProductIds);
        List<Guid> missingProductIds = appliedProductIds.Except(appliedProducts.Select(p => p.Id).ToList()).ToList();
        if (missingProductIds.Count > 0)
        {
            string missingIds = string.Join(", ", missingProductIds);
            throw new BadRequestException($"Không tìm thấy các sản phẩm: {missingIds} hoặc không còn kinh doanh để làm sản phẩm áp dụng khuyến mãi");
        }
        foreach (var appliedProductId in appliedProductIds)
        {
            promotion.AppliedProducts.Add(new PromotionAppliedProduct
            {
                ProductId = appliedProductId
            });
        }
    }

    private async Task ValidatePromotionCategoryScopeFromRequest(Guid? categoryId)
    {
        if (!categoryId.HasValue)
        {
            throw new BadRequestException("Khuyến mãi trên danh mục phải nhập danh mục cần được áp dụng");
        }
        if (!await _unitOfWork.CategoryRepository.ExistByIdAndIsNotDeleted(categoryId.Value))
        {
            throw new NotFoundException($"Không tìm thấy danh mục {categoryId.Value} hoặc danh mục đã bị xóa khỏi hệ thống");
        }
    }

    private async Task ValidatePromotionBrandScopeFromRequest(Guid? brandId)
    {
        if (!brandId.HasValue)
        {
            throw new BadRequestException("Khuyến mãi trên nhà cung cấp phải nhập nhà cung cấp cần được áp dụng");
        }
        if (!await _unitOfWork.BrandRepository.ExistsByIdAsync(brandId.Value))
        {
            throw new NotFoundException($"Không tìm thấy nhà cung cấp {brandId.Value} trong hệ thống");
        }
    }

    private async Task ValidatePromotionRequiredProductsFromRequest(List<CreatePromotionRequiredProductCommand> requiredProductCommands, PromotionRequiredProductLogic? requiredProductLogic, Promotion promotion)
    {
        if (requiredProductCommands.Count > 0)
        {
            if (requiredProductCommands.Count > 1 && requiredProductLogic is null)
            {
                throw new BadRequestException("Logic cho sản phẩm yêu cầu trong khuyến mãi bắt buộc phải có khi có nhiều hơn 1 sản phẩm yêu cầu");
            }
            List<Guid> requestRequiredProductIds = requiredProductCommands.Select(p => p.ProductId).ToList();
            List<Product> requestRequiredProducts = await _unitOfWork.ProductRepository.FindByIdsAndActiveAsync(requestRequiredProductIds);
            List<Guid> missingRequiredProductIds = requestRequiredProductIds.Except(requestRequiredProducts.Select(p => p.Id).ToList()).ToList();
            if (missingRequiredProductIds.Count > 0)
            {
                string missingIds = string.Join(", ", missingRequiredProductIds);
                throw new BadRequestException($"Các sản phẩm yêu cầu cho khuyến mãi không tồn tại hoặc không còn kinh doanh: {missingIds}");
            }
            foreach (var command in requiredProductCommands)
            {
                if (command.MinQuantity <= 0)
                {
                    throw new BadRequestException($"Sản phẩm yêu cầu tối thiểu cho khuyến mãi: {command.ProductId} không được phép có số lượng dưới hoặc bằng 0");
                }
                if (command.MaxQuantity.HasValue && command.MaxQuantity.Value <= command.MinQuantity)
                {
                    throw new BadRequestException($"Sản phẩm yêu cầu tối đa: {command.ProductId} cho khuyến mãi bắt buộc phải lớn hơn sản phẩm tối thiểu nếu có");
                }
                promotion.RequiredProducts.Add(new PromotionRequiredProduct
                {
                    ProductId = command.ProductId,
                    MinQuantity = command.MinQuantity,
                    MaxQuantity = command.MaxQuantity
                });
            }
        }
        else
        {
            requiredProductLogic = null;
        }
        promotion.RequiredProductLogic = requiredProductLogic;

    }


    public async Task<PromotionCalculationResult> CalculateForCheckoutAsync(List<string> codes, List<CheckoutItemCommand> items, Guid? userId, string? phone)
    {
        List<Guid> productIds = [.. items.Select(i => i.ProductId)];
        List<Product> products = await _unitOfWork.ProductRepository.FindByIdsAsync(productIds);

        HashSet<Guid> foundIds = [.. products.Select(p => p.Id)];
        List<Guid> missingIds = [.. productIds.Where(id => !foundIds.Contains(id))];
        if (missingIds.Count > 0)
        {
            throw new NotFoundException($"Không tìm thấy sản phẩm: {string.Join(", ", missingIds)}");
        }

        var productDict = products.ToDictionary(p => p.Id);
        List<CheckoutItemCommand> commands = [.. items.Select(i => new CheckoutItemCommand
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = productDict[i.ProductId].Price,
            CategoryId = productDict[i.ProductId].CategoryId,
            BrandId = productDict[i.ProductId].BrandId
        })];

        decimal subTotal = commands.Sum(c => c.UnitPrice * c.Quantity);
        return await CalculatePromotionAsync(codes, commands, subTotal, userId, phone);
    }

    public async Task<PromotionCalculationResult> CalculatePromotionAsync(List<string> codes, List<CheckoutItemCommand> checkoutItemCommands, decimal subTotal, Guid? userId, string? phone)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        codes = [.. codes.Select(c => c.Trim().ToUpper())];

        (List<Promotion> autoAppliedPromotions, List<Promotion> nonAutoAppliedPromotions) = await GetAutoAppliedAndNonAppliedPromotions(codes, now);

        List<Promotion> allActivePromotions = [.. autoAppliedPromotions, .. nonAutoAppliedPromotions];

        List<Promotion> eligiblePromotions = await GetEligiblePromotionsAsync(allActivePromotions, subTotal, userId, phone, checkoutItemCommands);
        HashSet<Guid> eligibleIds = [.. eligiblePromotions.Select(p => p.Id)];
        List<string> unappliedCodeMessages = [.. nonAutoAppliedPromotions.Where(p => !eligibleIds.Contains(p.Id)).Select(p => $"Khuyến mãi {p.Code} không đủ điều kiện áp dụng cho đơn hàng này")];


        List<Promotion> finalPromotions = GetPromotionsAfterStackabilityResolving(eligiblePromotions, checkoutItemCommands, subTotal);
        HashSet<Guid> finalIds = [.. finalPromotions.Select(p => p.Id)];
        List<string> stackabilityLostCodeMessages = [.. eligiblePromotions.Where(p => !string.IsNullOrEmpty(p.Code) && !finalIds.Contains(p.Id)).Select(p => $"Khuyến mãi {p.Code} không được áp dụng do có khuyến mãi khách tốt hơn được chọn")];

        unappliedCodeMessages.AddRange(stackabilityLostCodeMessages);

        PromotionCalculationResult result = GetPromotionCalculationResult(finalPromotions, checkoutItemCommands, subTotal, unappliedCodeMessages);
        return result;
    }

    private async Task<(List<Promotion>, List<Promotion>)> GetAutoAppliedAndNonAppliedPromotions(List<string> codes, DateTimeOffset now)
    {
        List<Promotion> autoAppliedPromotions = await _unitOfWork.PromotionRepository.FindActiveAutoApplyAsync(now);
        List<Promotion> nonAutoAppliedPromotions = codes.Count > 0 ? await _unitOfWork.PromotionRepository.FindActiveNonAutoApplyAsync(codes, now) : [];
        List<string> missingCodes = [.. codes.Except([.. nonAutoAppliedPromotions.Select(p => p.Code!)])];
        if (missingCodes.Count > 0)
        {
            string missing = string.Join(", ", missingCodes);
            throw new NotFoundException($"Không tìm thấy các mã khuyến mãi: {missing}");
        }
        return (autoAppliedPromotions, nonAutoAppliedPromotions);
    }

    private async Task<List<Promotion>> GetEligiblePromotionsAsync(List<Promotion> allActivePromotions, decimal subTotal, Guid? userId, string? phone, List<CheckoutItemCommand> checkoutItemCommands)
    {
        List<Promotion> eligiblePromotions = [];
        foreach (var activePromotion in allActivePromotions)
        {
            if (!IsMinOrderValueMet(activePromotion, subTotal)) continue;
            if (!IsPromotionUsageAvailable(activePromotion)) continue;
            if (!await IsPerUserUsageAvailable(activePromotion, userId, phone)) continue;
            if (!IsRequiredProductsMet(activePromotion, checkoutItemCommands)) continue;
            if (!IsScopeEligible(activePromotion, checkoutItemCommands)) continue;
            eligiblePromotions.Add(activePromotion);
        }
        return eligiblePromotions;
    }

    private static bool IsMinOrderValueMet(Promotion promotion, decimal subTotal)
    {
        return !promotion.MinOrderValue.HasValue || promotion.MinOrderValue.Value <= subTotal;
    }

    private static bool IsPromotionUsageAvailable(Promotion promotion)
    {
        return !promotion.MaxUsageCount.HasValue || promotion.UsageCount < promotion.MaxUsageCount.Value;
    }

    private async Task<bool> IsPerUserUsageAvailable(Promotion promotion, Guid? userId, string? phone)
    {
        if (!promotion.MaxUsagePerUser.HasValue)
        {
            return true;
        }
        int usageCount;
        if (userId.HasValue)
        {
            usageCount = await _unitOfWork.PromotionUsageRepository.CountByPromotionAndUserIdAsync(promotion.Id, userId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(phone))
        {
            usageCount = await _unitOfWork.PromotionUsageRepository.CountByPromotionAndPhoneAsync(promotion.Id, phone);
        }
        else
        {
            return false;
        }
        return usageCount < promotion.MaxUsagePerUser.Value;
    }

    private static bool IsRequiredProductsMet(Promotion promotion, List<CheckoutItemCommand> checkoutItemCommands)
    {
        if (promotion.RequiredProducts.Count == 0)
        {
            return true;
        }

        var checkoutItemDict = checkoutItemCommands.ToDictionary(i => i.ProductId, i => i.Quantity);

        bool Check(PromotionRequiredProduct required)
        {
            if (!checkoutItemDict.TryGetValue(required.ProductId, out int quantity))
            {
                return false;
            }
            if (quantity < required.MinQuantity)
            {
                return false;
            }
            if (required.MaxQuantity.HasValue && quantity > required.MaxQuantity.Value)
            {
                return false;
            }
            return true;
        }
        return promotion.RequiredProductLogic == PromotionRequiredProductLogic.Or ? promotion.RequiredProducts.Any(Check) : promotion.RequiredProducts.All(Check);
    }

    private static bool IsScopeEligible(Promotion promotion, List<CheckoutItemCommand> checkoutItemCommands)
    {
        List<CheckoutItemCommand> eligibles = [];
        switch (promotion.Scope)
        {
            case PromotionScope.Order:
                return true;
            case PromotionScope.Product:
                HashSet<Guid> appliedIds = [.. promotion.AppliedProducts.Select(ap => ap.ProductId)];
                eligibles = [.. checkoutItemCommands.Where(i => appliedIds.Contains(i.ProductId))];
                break;
            case PromotionScope.Category:
                eligibles = [.. checkoutItemCommands.Where(i => i.CategoryId == promotion.CategoryId)];
                break;
            case PromotionScope.Brand:
                eligibles = [.. checkoutItemCommands.Where(i => i.BrandId == promotion.BrandId)];
                break;
            default:
                return false;
        }
        if (eligibles.Count == 0)
        {
            return false;
        }
        if (promotion.MinAppliedQuantity.HasValue)
        {
            int totalQuantity = eligibles.Sum(i => i.Quantity);
            if (totalQuantity < promotion.MinAppliedQuantity.Value)
            {
                return false;
            }
        }
        return true;
    }

    private static List<Promotion> GetPromotionsAfterStackabilityResolving(List<Promotion> eligiblePromotions, List<CheckoutItemCommand> checkoutItemCommands, decimal subTotal)
    {
        if (eligiblePromotions.Count == 0)
        {
            return eligiblePromotions;
        }
        if (eligiblePromotions.All(p => p.IsStackable))
        {
            return eligiblePromotions;
        }
        List<Promotion> stackables = [.. eligiblePromotions.Where(p => p.IsStackable)];
        List<Promotion> nonstackables = [.. eligiblePromotions.Where(p => !p.IsStackable)];
        Promotion bestNonStackable = nonstackables.MaxBy(p => CalculateSinglePromotionWithDiscountType(p, checkoutItemCommands, subTotal))!;
        stackables.Add(bestNonStackable);
        return stackables;
    }

    // dùng để tính xem khuyến mãi nào là tốt nhất cho danh sách khuyến mãi không stackable
    private static decimal CalculateSinglePromotionWithDiscountType(Promotion promotion, List<CheckoutItemCommand> checkoutItemCommands, decimal subTotal)
    {
        return promotion.Type switch
        {
            PromotionType.FixedDiscount => promotion.DiscountValue ?? 0,
            PromotionType.PercentageDiscount => Math.Min(subTotal * (promotion.DiscountValue ?? 0) / 100, promotion.MaxDiscountValue ?? decimal.MaxValue),
            PromotionType.FreeItem => CalculateFreeItemValue(promotion, checkoutItemCommands),
            PromotionType.FixedPrice => Math.Max(subTotal - (promotion.DiscountValue ?? subTotal), 0),
            _ => 0
        };
    }

    private static decimal CalculateFreeItemValue(Promotion promotion, List<CheckoutItemCommand> checkoutItemCommands)
    {
        var productPriceDict = checkoutItemCommands.ToDictionary(i => i.ProductId, i => i.UnitPrice);
        return promotion.FreeProducts.Sum(p => productPriceDict.TryGetValue(p.ProductId, out decimal price) ? price * p.Quantity : 0);
    }

    private static PromotionCalculationResult GetPromotionCalculationResult(List<Promotion> promotions, List<CheckoutItemCommand> checkoutItemCommands, decimal subTotal, List<string> unappliedCodeMessages)
    {
        List<PromotionLineResult> appliedPromotions = [];
        List<FreeItemResult> totalFreeItemResults = [];
        foreach (var promotion in promotions)
        {
            PromotionLineResult lineResult = CalculatePromotionLineResult(promotion, checkoutItemCommands, subTotal);
            appliedPromotions.Add(lineResult);
            totalFreeItemResults.AddRange(lineResult.FreeItems);
        }
        decimal totalDiscountAmount = Math.Min(appliedPromotions.Sum(ap => ap.DiscountAmount), subTotal);
        return new PromotionCalculationResult(appliedPromotions, totalDiscountAmount, totalFreeItemResults, unappliedCodeMessages);
    }

    private static PromotionLineResult CalculatePromotionLineResult(Promotion promotion, List<CheckoutItemCommand> checkoutItemCommands, decimal subTotal)
    {
        decimal discountBase = GetDiscountBase(promotion, checkoutItemCommands, subTotal);
        (decimal discountAmount, List<FreeItemResult> freeItemResults) = ApplyDiscount(promotion, discountBase);
        return new PromotionLineResult(promotion.Id, promotion.Name, promotion.Code, discountAmount, freeItemResults, promotion.FreeItemPickCount);
    }

    private static decimal GetDiscountBase(Promotion promotion, List<CheckoutItemCommand> checkoutItemCommands, decimal subTotal)
    {
        return promotion.Scope switch
        {
            PromotionScope.Order => subTotal,
            PromotionScope.Product => checkoutItemCommands
                .Where(i => promotion.AppliedProducts.Select(ap => ap.ProductId).ToHashSet().Contains(i.ProductId))
                .Sum(i => i.UnitPrice * i.Quantity),
            PromotionScope.Category => checkoutItemCommands
                .Where(i => i.CategoryId == promotion.CategoryId)
                .Sum(i => i.UnitPrice * i.Quantity),
            PromotionScope.Brand => checkoutItemCommands
                .Where(i => i.BrandId == promotion.BrandId)
                .Sum(i => i.UnitPrice * i.Quantity),
            _ => 0
        };
    }

    private static (decimal, List<FreeItemResult>) ApplyDiscount(Promotion promotion, decimal discountBase)
    {
        decimal discountAmount = 0;
        List<FreeItemResult> freeItemResults = [];
        switch (promotion.Type)
        {
            case PromotionType.PercentageDiscount:
                discountAmount = Math.Min(discountBase * (promotion.DiscountValue ?? 0) / 100, promotion.MaxDiscountValue ?? decimal.MaxValue);
                break;
            case PromotionType.FixedDiscount:
                discountAmount = Math.Min(promotion.DiscountValue ?? 0, discountBase);
                break;
            case PromotionType.FreeItem:
                freeItemResults = [.. promotion.FreeProducts.Select(fp => new FreeItemResult(fp.ProductId, fp.Quantity))];
                break;
            case PromotionType.FixedPrice:
                discountAmount = Math.Max(discountBase - (promotion.DiscountValue ?? discountBase), 0);
                break;
        }
        return (discountAmount, freeItemResults);
    }


    // 1. Dành cho Admin: Gọi hàm xử lý chung với cờ checkActive = false
    public Task<Pagination<Product>> GetPromotionProductsForAdminAsync(Guid id, string? search, int page, int pageSize)
        => GetPromotionProductsInternalAsync(id, search, page, pageSize, checkActive: false);

    // 2. Dành cho Khách hàng: Gọi hàm xử lý chung với cờ checkActive = true
    public Task<Pagination<Product>> GetPromotionProductsForCustomerAsync(Guid id, string? search, int page, int pageSize)
        => GetPromotionProductsInternalAsync(id, search, page, pageSize, checkActive: true);

    // 3. Hàm xử lý logic lõi (Core Logic)
    private async Task<Pagination<Product>> GetPromotionProductsInternalAsync(
        Guid id,
        string? search,
        int page,
        int pageSize,
        bool checkActive)
    {
        // Tối ưu: Lấy thông tin KM một lần duy nhất.
        // Bạn nên đảm bảo hàm FindByIdIncludeAppliedProductsAsync trong Repository sử dụng .AsNoTracking() để tăng tốc độ truy vấn.
        var promotion = await _unitOfWork.PromotionRepository.FindByIdIncludeAppliedProductsAsync(id)
            ?? throw new NotFoundException("Không tìm thấy chương trình khuyến mãi.");

        // Tối ưu hiệu suất: Kiểm tra điều kiện ngắt sớm cho Customer
        if (checkActive)
        {
            var now = DateTimeOffset.Now;
            if (now < promotion.StartDate || now > promotion.EndDate)
            {
                throw new BadRequestException("Khuyến mãi này hiện không khả dụng hoặc đã hết hạn.");
            }
        }

        // Tối ưu bộ nhớ: Trích xuất ID trực tiếp từ Navigation Property
        var appliedIds = promotion.AppliedProducts.Select(ap => ap.ProductId).ToList();

        // Tối ưu: Giải nén Tuple từ Repository một cách trực quan
        var (products, totalCount) = await _unitOfWork.ProductRepository.GetProductsByPromotionScopeAsync(
            promotion.Scope,
            promotion.CategoryId,
            promotion.BrandId,
            appliedIds,
            search,
            page,
            pageSize);

        // Trả về object phân trang đồng nhất
        return new Pagination<Product>
        {
            Items = products,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    // ===================== List Promotion ==================================
    // TechExpress.Service/Services/PromotionService.cs

    public async Task<Pagination<Promotion>> GetPromotionsPagedAsync(
        string? search,
        bool? status,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        string sortBy,
        bool isDescending,
        int page,
        int pageSize)
    {
        var (promotions, totalCount) = await _unitOfWork.PromotionRepository.FindPromotionsPagedAsync(
            search, status, fromDate, toDate, sortBy, isDescending, page, pageSize);

        return new Pagination<Promotion>
        {
            Items = promotions,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Promotion> HandleDisablePromotion(Guid promotionId)
    {
        var promotion = await _unitOfWork.PromotionRepository.FindByIdWithTrackingAsync(promotionId) ?? throw new NotFoundException($"Không tìm thấy khuyến mãi: {promotionId}");
        if (!promotion.IsActive)
        {
            throw new BadRequestException($"Khuyến mãi đã ở trạng thái được tắt, không thể tiếp tục thực hiện hành động này");
        }
        promotion.IsActive = false;
        promotion.UpdatedAt = DateTimeOffset.Now;
        await _unitOfWork.SaveChangesAsync();
        var disabledPromotion = await _unitOfWork.PromotionRepository.FindByIdIncludeRequiredProductsIncludeFreeProductsIncludeAppliedProductsWithSplitQueryAsync(promotionId) ?? throw new NotFoundException($"Không tìm thấy mã khuyến mãi: {promotionId}");
        return disabledPromotion;
    }


}
