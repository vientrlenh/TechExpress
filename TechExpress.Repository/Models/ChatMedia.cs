using System;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class ChatMedia
{
    public long Id { get; set; }

    public required Guid MessageId { get; set; }

    public required string MediaUrl { get; set; }

    public required ChatMediaType Type { get; set; }

    public ChatMessage Message { get; set; } = null!;
}
