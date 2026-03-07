using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Responses
{
    public class AppliedPromotionResponse
    {
        public Guid PromotionId { get; set; }
        public string? PromotionCode { get; set; }

        public PromotionType PromotionType { get; set; }

       
        public string? PromotionName { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
