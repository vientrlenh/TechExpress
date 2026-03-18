using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Enums;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services;

public class TicketService(UnitOfWork unitOfWork)
{
    private readonly UnitOfWork _unitOfWork = unitOfWork;

    public async Task<Ticket> HandleCreateTicketForAuthenticatedUser(
        Guid userId,
        string title,
        string description,
        string message,
        TicketType type,
        Guid? customPCId,
        Guid? orderId,
        long? orderItemId,
        List<string> attachmentUrls)
    {
        
        var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId)
            ?? throw new NotFoundException($"Không tìm thấy người dùng: {userId}");

        if (customPCId.HasValue)
        {
            var customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPCId.Value)
                ?? throw new NotFoundException($"Không tìm thấy cấu hình PC: {customPCId}");
            if (customPC.UserId != userId)
                throw new ForbiddenException("Bạn không có quyền sử dụng cấu hình PC này");
        }

        if (orderId.HasValue)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdAsync(orderId.Value) ?? throw new NotFoundException($"Không tìm thấy đơn hàng {orderId.Value}");
            if (order.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền thực hiện gửi ticket với đơn hàng này");
            }
        }

        if (orderItemId.HasValue)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.FindByIdIncludeOrderAsync(orderItemId.Value) ?? throw new NotFoundException("Không tìm thấy sản phẩm trong đơn hàng");
            if (orderItem.Order.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền gửi ticket cho sản phẩm trong đơn hàng này");
            }
        }

        var fullName = ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim();

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = fullName,
            Phone = user.Phone,
            Title = title,
            Description = description,
            Type = type,
            Status = TicketStatus.Open
        };

        ValidateAndGetCorrectTicketType(type, customPCId, orderId, orderItemId, ticket);

        await _unitOfWork.TicketRepository.AddAsync(ticket);

        var initialMessage = new TicketMessage
        {
            TicketId = ticket.Id,
            UserId = userId,
            Content = message,
            IsStaffMessage = false,
            Attachments = [.. (attachmentUrls ?? []).Select(url => new TicketAttachment { FileUrl = url })]
        };

        await _unitOfWork.TicketMessageRepository.AddAsync(initialMessage);
        await _unitOfWork.SaveChangesAsync();

        return await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticket.Id)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticket.Id}");
    }


    public async Task<Ticket> HandleCreateTicketForUnauthenticatedUser(
        string? sessionId,
        string? fullName,
        string? phone,
        string title,
        string description,
        string message,
        TicketType type,
        Guid? customPCId,
        Guid? orderId,
        long? orderItemId,
        List<string> attachments
    )
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
        {
            throw new BadRequestException("Cần nhập đầy đủ họ tên và số điện thoại để thực hiện gửi ticket");
        }
        if (customPCId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new BadRequestException("Yêu cầu được gửi thiếu sessionId để kiểm tra thông tin cấu hình PC");
            var customPC = await _unitOfWork.CustomPCRepository.FindByIdAsync(customPCId.Value) ?? throw new NotFoundException($"Không tìm thấy cấu hình PC {customPCId.Value}");
            if (customPC.SessionId != sessionId)
            {
                throw new ForbiddenException("Bạn không có quyền gửi ticket cho cấu hình PC này");
            }
        }
        if (orderId.HasValue)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdAsync(orderId.Value) ?? throw new NotFoundException($"Không tìm thấy đơn hàng {orderId.Value}");
            if (order.TrackingPhone != phone)
            {
                throw new ForbiddenException("Bạn không có quyền gửi ticket cho đơn hàng này");
            }
        }
        if (orderItemId.HasValue)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.FindByIdIncludeOrderAsync(orderItemId.Value) ?? throw new NotFoundException($"Không tìm thấy sản phẩm trong đơn hàng {orderItemId.Value}");
            if (orderItem.Order.TrackingPhone != phone)
            {
                throw new ForbiddenException("Bạn không có quyền gửi ticket cho sản phẩm trong đơn hàng này");
            }
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Phone = phone,
            Title = title,
            Description = description,
            Type = type,
            Status = TicketStatus.Open
        };

        ValidateAndGetCorrectTicketType(type, customPCId, orderId, orderItemId, ticket);

        await _unitOfWork.TicketRepository.AddAsync(ticket);

        var initialMessage = new TicketMessage
        {
            TicketId = ticket.Id,
            Content = message,
            IsStaffMessage = false,
            Attachments = [.. (attachments ?? []).Select(url => new TicketAttachment { FileUrl = url })]
        };

        await _unitOfWork.TicketMessageRepository.AddAsync(initialMessage);
        await _unitOfWork.SaveChangesAsync();

        return await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticket.Id)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticket.Id}");
    }

    // ── Public: paginated list of tickets ──────────────────────────────────
    public async Task<Pagination<Ticket>> HandleGetAllTickets(
        TicketStatus? status,
        TicketSortBy sortBy,
        SortDirection sortDirection,
        int page,
        int size)
    {
        bool sortAsc = sortDirection == SortDirection.Asc;
        var (items, total) = await _unitOfWork.TicketRepository.FindPaginatedAsync(
            status, sortAsc, page, size);

        return new Pagination<Ticket>
        {
            Items = items,
            PageNumber = page,
            PageSize = size,
            TotalCount = total
        };
    }

    // ── Customer: paginated ticket list ──────────────────────────────────
    public async Task<Pagination<Ticket>> HandleGetTicketsForCustomer(
        Guid userId,
        TicketStatus? status,
        TicketSortBy sortBy,
        SortDirection sortDirection,
        int page,
        int size)
    {
        bool sortAsc = sortDirection == SortDirection.Asc;
        var (items, total) = await _unitOfWork.TicketRepository.FindPaginatedByUserIdAsync(
            userId, status, sortAsc, page, size);

        return new Pagination<Ticket>
        {
            Items = items,
            PageNumber = page,
            PageSize = size,
            TotalCount = total
        };
    }

    // ── Guest: paginated ticket list by phone ─────────────────────────────
    public async Task<Pagination<Ticket>> HandleGetTicketsForGuest(
        string? phone,
        TicketStatus? status,
        TicketSortBy sortBy,
        SortDirection sortDirection,
        int page,
        int size)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new BadRequestException("Vui lòng cung cấp số điện thoại để xem danh sách ticket");

        bool sortAsc = sortDirection == SortDirection.Asc;
        var (items, total) = await _unitOfWork.TicketRepository.FindPaginatedByPhoneAsync(
            phone, status, sortAsc, page, size);

        return new Pagination<Ticket>
        {
            Items = items,
            PageNumber = page,
            PageSize = size,
            TotalCount = total
        };
    }

    // ── Customer: ticket detail with messages ─────────────────────────────
    public async Task<Ticket> HandleGetMyTicketDetail(Guid userId, Guid ticketId)
    {
        var ticket = await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        if (ticket.UserId != userId)
            throw new ForbiddenException("Bạn không có quyền truy cập ticket này");

        return ticket;
    }

    // ── Guest: ticket detail by phone ─────────────────────────────────────
    public async Task<Ticket> HandleGetTicketDetailForGuest(string? phone, Guid ticketId)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new BadRequestException("Vui lòng cung cấp số điện thoại để xem thông tin ticket");

        var ticket = await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        if (ticket.Phone != phone)
            throw new ForbiddenException("Số điện thoại không khớp với thông tin ticket này");

        return ticket;
    }

    // ── Staff / Admin: any ticket detail ─────────────────────────────────
    public async Task<Ticket> HandleGetTicketDetail(Guid ticketId)
    {
        return await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");
    }

    // ── Guest: add a reply message (phone-verified) ──────────────────────
    public async Task<(TicketMessage Message, Ticket Ticket)> HandleReplyToTicketForUnauthenticatedUser(
        Guid ticketId,
        string? phone,
        string content,
        List<string>? attachmentUrls)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new BadRequestException("Vui lòng cung cấp số điện thoại để xác thực");

        var ticket = await _unitOfWork.TicketRepository.FindByIdWithTrackingAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            throw new BadRequestException($"Không thể phản hồi ticket đã được {ticket.Status}");

        if (ticket.Phone != phone)
            throw new ForbiddenException("Số điện thoại không khớp với thông tin ticket này");

        var message = new TicketMessage
        {
            TicketId = ticketId,
            UserId = null,
            Content = content,
            IsStaffMessage = false,
            Attachments = (attachmentUrls ?? [])
                .Select(url => new TicketAttachment { FileUrl = url })
                .ToList()
        };

        await _unitOfWork.TicketMessageRepository.AddAsync(message);
        ticket.UpdatedAt = DateTimeOffset.Now;
        await _unitOfWork.SaveChangesAsync();

        var updatedTicket = await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        return (message, updatedTicket);
    }

    // ── Authenticated: add a reply message ──────────────────────────────
    // Returns (new message, updated ticket)
    public async Task<(TicketMessage Message, Ticket Ticket)> HandleReplyToTicketForAuthenticatedUser(
        Guid userId,
        Guid ticketId,
        string content,
        List<string>? attachmentUrls,
        bool isStaff)
    {
        var ticket = await _unitOfWork.TicketRepository.FindByIdWithTrackingAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            throw new BadRequestException($"Không thể phản hồi ticket đã được {ticket.Status}");

        if (!isStaff && ticket.UserId != userId)
            throw new ForbiddenException("Bạn không có quyền phản hồi ticket này");

        var message = new TicketMessage
        {
            TicketId = ticketId,
            UserId = userId,
            Content = content,
            IsStaffMessage = isStaff,
            Attachments = (attachmentUrls ?? [])
                .Select(url => new TicketAttachment { FileUrl = url })
                .ToList()
        };

        await _unitOfWork.TicketMessageRepository.AddAsync(message);

        ticket.UpdatedAt = DateTimeOffset.Now;

        await _unitOfWork.SaveChangesAsync();

        var updatedTicket = await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        return (message, updatedTicket);
    }

    // ── Staff / Admin: update ticket status (non-terminal only) ─────────
    // Returns updated ticket
    public async Task<Ticket> HandleUpdateTicketStatus(
        Guid ticketId,
        TicketStatus newStatus)
    {
        if (newStatus == TicketStatus.Resolved || newStatus == TicketStatus.Closed)
            throw new BadRequestException(
                "Sử dụng endpoint hoàn thành ticket (complete) để đặt trạng thái Resolved hoặc Closed");

        var ticket = await _unitOfWork.TicketRepository.FindByIdWithTrackingAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        if (ticket.Status == TicketStatus.Closed)
            throw new BadRequestException("Không thể cập nhật trạng thái ticket đã đóng");

        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTimeOffset.Now;

        await _unitOfWork.SaveChangesAsync();

        var updatedTicket = await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        return updatedTicket;
    }

    // ── Staff / Admin: complete a ticket (Resolved or Closed) ───────────
    // Returns completed ticket
    public async Task<Ticket> HandleCompleteTicket(
        Guid staffId,
        Guid ticketId,
        TicketStatus targetStatus)
    {
        if (targetStatus != TicketStatus.Resolved && targetStatus != TicketStatus.Closed)
            throw new BadRequestException("Trạng thái hoàn thành phải là Resolved hoặc Closed");

        var ticket = await _unitOfWork.TicketRepository.FindByIdWithTrackingAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        if (ticket.Status == TicketStatus.Closed)
            throw new BadRequestException("Ticket đã đóng, không thể thực hiện thao tác này");

        if (ticket.Status == TicketStatus.Resolved && targetStatus == TicketStatus.Resolved)
            throw new BadRequestException("Ticket đã ở trạng thái Resolved");

        ticket.Status = targetStatus;
        ticket.CompletedByUserId = staffId;
        ticket.UpdatedAt = DateTimeOffset.Now;

        if (targetStatus == TicketStatus.Resolved)
        {
            ticket.ResolvedAt = DateTimeOffset.Now;
        }
        else
        {
            if (!ticket.ResolvedAt.HasValue)
                ticket.ResolvedAt = DateTimeOffset.Now;
            ticket.ClosedAt = DateTimeOffset.Now;
        }

        await _unitOfWork.SaveChangesAsync();

        var completedTicket = await _unitOfWork.TicketRepository.FindByIdFullJoinWithSplitQueryAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        return completedTicket;
    }

    private static void ValidateAndGetCorrectTicketType(TicketType type, Guid? customPCId, Guid? orderId, long? orderItemId, Ticket ticket)
    {
        switch (type)
        {
            case TicketType.OrderIssue:
                if (!orderId.HasValue)
                {
                    throw new BadRequestException("Ticket cần đính kèm thông tin đơn hàng có vấn đề");
                }
                customPCId = null;
                orderItemId = null;
                break;
            case TicketType.WarrantyRequest:
                if (!orderItemId.HasValue)
                {
                    throw new BadRequestException("Ticket yêu cầu thông tin sản phẩm trong đơn hàng cho việc bảo hành");
                }
                customPCId = null;
                orderId = null;
                break;
            case TicketType.BuildAdvice:
                orderId = null;
                orderItemId = null;
                break;
            case TicketType.CompatibilityQuestion:
                orderId = null;
                orderItemId = null;
                break;
            case TicketType.TechnicalSupport:
                orderId = null;
                orderItemId = null;
                customPCId = null;
                break;
            case TicketType.Other:
                break;
            default:
                throw new BadRequestException("Loại ticket này hiện tại chưa được hỗ trợ");
        }
        ticket.CustomPCId = customPCId;
        ticket.OrderId = orderId;
        ticket.OrderItemId = orderItemId;
    }
}
