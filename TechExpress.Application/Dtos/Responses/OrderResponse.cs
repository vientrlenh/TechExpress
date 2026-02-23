using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Responses
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PaidType PaidType { get; set; }
        public string? ReceiverFullName { get; set; }
        public string? ReceiverEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public string TrackingPhone { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<OrderItemResponse> Items { get; set; } = [];

        //// Trả về danh sách các kỳ hạn thanh toán
        public ICollection<InstallmentResponse> Installments { get; set; } = [];

        // Trả về duy nhất 1 thông tin tóm tắt gói trả góp
        //public InstallmentResponse? Installment { get; set; }
    }
}
