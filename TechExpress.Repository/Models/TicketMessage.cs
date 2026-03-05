using System;

namespace TechExpress.Repository.Models;

public class TicketMessage
{
    public long Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid? UserId { get; set; }

    public required string Content { get; set; }

    public bool IsStaffMessage { get; set; }

    public DateTimeOffset SentAt { get; private set; } = DateTimeOffset.Now;

    public Ticket Ticket { get; set; } = null!;

    public User? User { get; set; }

    public ICollection<TicketAttachment> Attachments { get; set; } = [];
}
