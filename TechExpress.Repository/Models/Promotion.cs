using System;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class Promotion
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Code { get; set; } // để null nếu áp dụng tự động hoặc để giá trị nếu áp dụng thủ công

    public required string Description { get; set; }

    public required PromotionType Type { get; set; }

    public required PromotionScope Scope { get; set; }

    public decimal? DiscountValue { get; set; }

    public decimal? MaxDiscountValue { get; set; } // ví dụ giảm 10% giá trị đơn hàng tối đa 500k

    public decimal? MinOrderValue { get; set; }

    public ICollection<PromotionRequiredProduct> RequiredProducts { get; set; } = [];

    public PromotionRequiredProductLogic? RequiredProductLogic { get; set; }

    public ICollection<PromotionFreeProduct> FreeProducts { get; set; } = [];

    public int? FreeItemPickCount { get; set; }

    public Guid? CategoryId { get; set; } // dành cho scope là category (khuyến mãi kệ hàng)

    public Guid? BrandId { get; set; } // dành cho scope là brand (khuyến mãi trên 1 brand nào đó)

    public ICollection<PromotionAppliedProduct> AppliedProducts { get; set; } = [];

    public int? MinAppliedQuantity { get; set; } // tối thiểu số lượng sản phẩm cần để áp dụng khuyến mãi

    public int? MaxUsageCount { get; set; } // không set giá trị thì tức là khuyến mãi không có giới hạn sử dụng

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
