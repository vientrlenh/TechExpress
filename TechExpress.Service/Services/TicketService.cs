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

    // ── Guest / logged-in: custom PC build ticket (existing) ─────────────
    public async Task<Ticket> HandleCreateCustomPCBuildTicket(
        string? userIdStr,
        string? fullName,
        string? phone,
        string title,
        string message,
        Guid? customPCId,
        List<string>? attachmentUrls)
    {
        Guid? userId = null;

        if (userIdStr is not null)
        {
            userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value)
                ?? throw new NotFoundException($"Không tìm thấy người dùng: {userId}");
            fullName = ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim();
            phone = user.Phone;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
                throw new BadRequestException("Họ tên và số điện thoại là bắt buộc đối với người dùng chưa đăng nhập");
        }

        if (customPCId.HasValue)
        {
            if (userId is null)
                throw new BadRequestException("Người dùng cần đăng nhập để đính kèm cấu hình PC");
            var customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPCId.Value)
                ?? throw new NotFoundException($"Không tìm thấy cấu hình PC: {customPCId}");
            if (customPC.UserId != userId)
                throw new ForbiddenException("Bạn không có quyền sử dụng cấu hình PC này");
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = fullName,
            Phone = phone,
            Title = title,
            Description = message,
            Type = TicketType.BuildAdvice,
            Status = TicketStatus.Open
        };

        if (customPCId.HasValue)
            ticket.CustomPCId = customPCId;

        await _unitOfWork.TicketRepository.AddAsync(ticket);

        var initialMessage = new TicketMessage
        {
            TicketId = ticket.Id,
            UserId = userId,
            Content = message,
            IsStaffMessage = false,
            Attachments = (attachmentUrls ?? [])
                .Select(url => new TicketAttachment { FileUrl = url })
                .ToList()
        };

        await _unitOfWork.TicketMessageRepository.AddAsync(initialMessage);
        await _unitOfWork.SaveChangesAsync();

        return await _unitOfWork.TicketRepository.FindByIdIncludeMessagesWithAttachmentsAsync(ticket.Id)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticket.Id}");
    }

    // ── Customer: create general support ticket (authenticated) ──────────
    public async Task<Ticket> HandleCreateTicket(
        Guid userId,
        string title,
        string message,
        TicketType type,
        Guid? customPCId,
        List<string>? attachmentUrls)
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

        var fullName = ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim();

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = fullName,
            Phone = user.Phone,
            Title = title,
            Description = message,
            Type = type,
            Status = TicketStatus.Open,
            CustomPCId = customPCId
        };

        await _unitOfWork.TicketRepository.AddAsync(ticket);

        var initialMessage = new TicketMessage
        {
            TicketId = ticket.Id,
            UserId = userId,
            Content = message,
            IsStaffMessage = false,
            Attachments = (attachmentUrls ?? [])
                .Select(url => new TicketAttachment { FileUrl = url })
                .ToList()
        };

        await _unitOfWork.TicketMessageRepository.AddAsync(initialMessage);
        await _unitOfWork.SaveChangesAsync();

        return await _unitOfWork.TicketRepository.FindByIdIncludeMessagesWithAttachmentsAsync(ticket.Id)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticket.Id}");
    }

    // ── Public: paginated list of tickets ──────────────────────────────────
    public async Task<Pagination<Ticket>> HandleGetMyTickets(
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

    // ── Customer: ticket detail with messages ─────────────────────────────
    public async Task<Ticket> HandleGetMyTicketDetail(Guid userId, Guid ticketId)
    {
        var ticket = await _unitOfWork.TicketRepository.FindByIdIncludeMessagesWithAttachmentsAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        if (ticket.UserId != userId)
            throw new ForbiddenException("Bạn không có quyền truy cập ticket này");

        return ticket;
    }

    // ── Staff / Admin: any ticket detail ─────────────────────────────────
    public async Task<Ticket> HandleGetTicketDetail(Guid ticketId)
    {
        return await _unitOfWork.TicketRepository.FindByIdIncludeMessagesWithAttachmentsAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");
    }

    // ── Both: add a reply message ────────────────────────────────────────
    // Returns (new message, updated ticket, notificationTargetUserId?)
    public async Task<(TicketMessage Message, Ticket Ticket)> HandleReplyToTicket(
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

        {
            await Task.CompletedTask;
            /*
                UserId = notifyUserId.Value,
                Type = NotificationType.TicketAlert,
                Title = "Ticket có phản hồi mới",
                Message = $"Ticket \"{ticket.Title}\" vừa nhận được phản hồi mới.",
                ReferenceId = ticketId,
                ReferenceType = NotificationReferenceType.Ticket
            };
            */
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedTicket = await _unitOfWork.TicketRepository.FindByIdIncludeMessagesWithAttachmentsAsync(ticketId)
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

        if (false)
        {
            /*
            {
                UserId = notifyUserId.Value,
                Type = NotificationType.TicketAlert,
                Title = "Trạng thái ticket đã thay đổi",
                Message = $"Ticket \"{ticket.Title}\" đã chuyển sang trạng thái {newStatus}.",
                ReferenceId = ticketId,
                ReferenceType = NotificationReferenceType.Ticket
            };
            */
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedTicket = await _unitOfWork.TicketRepository.FindByIdIncludeMessagesWithAttachmentsAsync(ticketId)
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

        if (false)
        {
            var statusLabel = targetStatus == TicketStatus.Resolved ? "đã được giải quyết" : "đã được đóng";
            /*
            {
                UserId = notifyUserId.Value,
                Type = NotificationType.TicketAlert,
                Title = "Ticket của bạn đã được xử lý",
                Message = $"Ticket \"{ticket.Title}\" {statusLabel}.",
                ReferenceId = ticketId,
                ReferenceType = NotificationReferenceType.Ticket
            */
        }

        await _unitOfWork.SaveChangesAsync();

        var completedTicket = await _unitOfWork.TicketRepository.FindByIdIncludeMessagesWithAttachmentsAsync(ticketId)
            ?? throw new NotFoundException($"Không tìm thấy ticket: {ticketId}");

        return completedTicket;
    }
}
