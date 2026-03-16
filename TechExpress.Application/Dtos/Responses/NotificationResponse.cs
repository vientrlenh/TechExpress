using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Responses;

public record NotificationResponse(
    long Id,
    string Title,
    string Message,
    NotificationType Type,
    Guid? ReferenceId,
    NotificationReferenceType? ReferenceType,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt
);
