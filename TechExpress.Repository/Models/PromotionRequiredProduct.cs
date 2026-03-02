using System;

namespace TechExpress.Repository.Models;

public class PromotionRequiredProduct
{
    public long Id { get; set; }

    public Guid PromotionId { get; set; }

    public Guid ProductId { get; set; }

    public int MinQuantity { get; set; }

    public int? MaxQuantity { get; set; }

    public Promotion Promotion { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
