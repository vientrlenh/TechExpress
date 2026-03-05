using System;

namespace TechExpress.Repository.Models;

public class PromotionUsage
{
    public long Id { get; set; }

    public Guid PromotionId { get; set; }

    public Guid? UserId { get; set; }

    public Guid OrderId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public decimal DiscountAmount { get; set; }

    public DateTimeOffset UsedAt { get; private set; } = DateTimeOffset.Now;

    public Promotion Promotion { get; set; } = null!;

    public User? User { get; set; }

    public Order Order { get; set; } = null!;
}
