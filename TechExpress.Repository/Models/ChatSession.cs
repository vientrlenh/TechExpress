using System;

namespace TechExpress.Repository.Models;

public class ChatSession
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = [];

    public bool IsEscalated { get; set; }

    public bool IsClosed { get; set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public User? User { get; set; }
}
