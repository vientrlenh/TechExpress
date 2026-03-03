using System;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class Promotion
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Code { get; set; }

    public required string Description { get; set; }

    public required PromotionType Type { get; set; }

    public required PromotionScope Scope { get; set; }

    public decimal? DiscountValue { get; set; }

    public decimal? MaxDiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }

    public ICollection<PromotionRequiredProduct> RequiredProducts { get; set; } = [];

    public PromotionRequiredProductLogic? RequiredProductLogic { get; set; }

    public ICollection<PromotionFreeProduct> FreeProducts { get; set; } = [];

    public int? FreeItemPickCount { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? BrandId { get; set; }

    public ICollection<PromotionAppliedProduct> AppliedProducts { get; set; } = [];

    public int? MinAppliedQuantity { get; set; }

    public int? MaxUsageCount { get; set; }

    public int UsageCount { get; set; }

    public int? MaxUsagePerUser { get; set; }

    public required DateTimeOffset StartDate { get; set; }

    public required DateTimeOffset EndDate { get; set; }

    public bool IsStackable { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public ICollection<PromotionUsage> Usages { get; set; } = [];

    public Category? Category { get; set; }

    public Brand? Brand { get; set; }

}
