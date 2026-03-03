using System;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class Notification
{
    public long Id { get; set; }

    public Guid UserId { get; set; }

    public required NotificationType Type { get; set; }

    public required string Title { get; set; }

    public required string Message { get; set; }

    public Guid? ReferenceId { get; set; }

    public NotificationReferenceType? ReferenceType { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

    public User User { get; set; } = null!;
}
