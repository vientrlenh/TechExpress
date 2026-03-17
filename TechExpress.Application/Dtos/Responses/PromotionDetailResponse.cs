using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Responses
{
    public class PromotionDetailResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Code { get; set; }

        public string Description { get; set; } = null!;

        public PromotionType DiscountType { get; set; }

        public decimal? DiscountValue { get; set; }

        public decimal? MaxDiscountValue { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public int? UsageLimit { get; set; }

        public int? UsagePerUser { get; set; }

        public bool Status { get; set; }

        public bool IsExpired { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public PromotionScope Scope { get; set; }

        public decimal? MinOrderValue { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public int? MinAppliedQuantity { get; set; }

        public PromotionRequiredProductLogic? RequiredProductLogic { get; set; }

        public int? FreeItemPickCount { get; set; }

        public bool IsStackable { get; set; }

        public int UsageCount { get; set; }

        //public List<PromotionRequiredProductDetailResponse> RequiredProducts { get; set; } = [];

        //public List<PromotionFreeProductDetailResponse> FreeProducts { get; set; } = [];

        //public List<PromotionAppliedProductDetailResponse> AppliedProducts { get; set; } = [];
    }

    //public class PromotionRequiredProductDetailResponse
    //{
    //    public Guid ProductId { get; set; }

    //    public int MinQuantity { get; set; }

    //    public int? MaxQuantity { get; set; }
    //}

    //public class PromotionFreeProductDetailResponse
    //{
    //    public Guid ProductId { get; set; }

    //    public int Quantity { get; set; }
    //}

    //public class PromotionAppliedProductDetailResponse
    //{
    //    public Guid ProductId { get; set; }
    //}
}