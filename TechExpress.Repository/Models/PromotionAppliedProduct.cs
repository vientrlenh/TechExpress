using System;

namespace TechExpress.Repository.Models;

public class PromotionAppliedProduct
{
    public long Id { get; set; }

    public Guid PromotionId { get; set; }

    public Guid ProductId { get; set; }

    public Promotion Promotion { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
