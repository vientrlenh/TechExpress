using System;

namespace TechExpress.Repository.Models;

public class ChatMessage
{
    public Guid Id { get; set; }

    public required Guid SessionId { get; set; }

    public Guid? SentById { get; set; }

    public string? SentByFullName { get; set; }

    public required string Message { get; set; }

    public bool IsAiMessage { get; set; }

    public ICollection<ChatMedia> Medias { get; set; } = [];

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

    public ChatSession Session { get; set; } = null!;

    public User? SentBy { get; set; }
}
