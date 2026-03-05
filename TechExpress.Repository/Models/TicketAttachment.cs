using System;

namespace TechExpress.Repository.Models;

public class TicketAttachment
{
    public long Id { get; set; }

    public long MessageId { get; set; }

    public required string FileUrl { get; set; }

    public DateTimeOffset UploadedAt { get; private set; } = DateTimeOffset.Now;

    public TicketMessage Message { get; set; } = null!;
}
