using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Responses;

public record TicketAttachmentResponse(
    long Id,
    string FileUrl,
    DateTimeOffset UploadedAt
);

public record TicketMessageResponse(
    long Id,
    Guid TicketId,
    Guid? UserId,
    string Content,
    bool IsStaffMessage,
    List<TicketAttachmentResponse> Attachments,
    DateTimeOffset SentAt
);

public record TicketListItemResponse(
    Guid Id,
    Guid? UserId,
    string Title,
    string Content,
    TicketStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record TicketResponse(
    Guid Id,
    Guid? UserId,
    string? FullName,
    string? Phone,
    string Title,
    string Description,
    TicketType Type,
    TicketStatus Status,
    TicketPriority Priority,
    Guid? CustomPCId,
    CustomPCResponse? CustomPC,
    List<TicketMessageResponse> Messages,
    Guid? CompletedByUserId,
    string? CompletedByName,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CompleteTicketResponse(
    Guid Id,
    string Title,
    TicketStatus Status,
    Guid? CompletedByUserId,
    string? CompletedByName,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset UpdatedAt
);

public record NotificationResponse(
    long Id,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    Guid? ReferenceId,
    string? ReferenceType,
    bool IsRead,
    DateTimeOffset CreatedAt
);
