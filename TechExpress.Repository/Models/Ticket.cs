using System;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class Ticket
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required TicketType Type { get; set; }

    public required TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public Guid? CustomPCId { get; set; }

    public Guid? OrderId { get; set; }

    public long? OrderItemId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public string? Result { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public User? User { get; set; }

    public CustomPC? CustomPC { get; set; }

    public Order? Order { get; set; }

    public OrderItem? OrderItem { get; set; }

    public User? AssignedTo { get; set; }

    public User? CompletedBy { get; set; }

    public ICollection<TicketMessage> Messages { get; set; } = [];
}
