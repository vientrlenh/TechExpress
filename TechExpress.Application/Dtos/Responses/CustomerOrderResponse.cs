using TechExpress.Application.DTOs.Responses;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Responses
{
    public class CustomerOrderListItemResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class CustomerOrderDetailResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
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
        public PaymentStatus? LatestPaymentStatus { get; set; }
        public PaymentMethod? LatestPaymentMethod { get; set; }
        public ICollection<OrderItemResponse> Items { get; set; } = [];
        public ICollection<PaymentSummaryResponse> Payments { get; set; } = [];
    }

    public class PaymentSummaryResponse
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
    }
}
