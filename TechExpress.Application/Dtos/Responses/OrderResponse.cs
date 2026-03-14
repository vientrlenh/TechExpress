using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Responses
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }

        public decimal DiscountAmount { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PaidType PaidType { get; set; }
        public string? ReceiverFullName { get; set; }
        public string? ReceiverEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public string TrackingPhone { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public Guid? CreatedByStaffId { get; set; }
        public string? ReceiverIdentityCard { get; set; }
        public int? InstallmentDurationMonth { get; set; }
        // Tách làm 2 danh sách riêng biệt
        public List<OrderItemResponse> PurchasedItems { get; set; } = new(); // Sản phẩm mua
        public List<OrderItemResponse> GiftItems { get; set; } = new();      // Sản phẩm tặng

        public ICollection<InstallmentResponse> Installments { get; set; } = [];

        // Thêm danh sách này để hiện các mã đã áp dụng
        public List<AppliedPromotionResponse>? AppliedPromotions { get; set; }
    }
}
