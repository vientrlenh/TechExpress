using System;

namespace TechExpress.Repository.Models;

public class CustomPC
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? SessionId { get; set; }

    public required string Name { get; set; }

    public bool IsStaffAccessible { get; set;   }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public User? User { get; set; }

    public ICollection<CustomPCItem> Items { get; set; } = [];
}
