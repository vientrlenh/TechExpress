using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Responses
{
    public class OrderItemDetailResponse
    {
        public long Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        // Product basic info
        public ProductListResponse? Product { get; set; }
    }

    public class OrderDetailResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
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
        public string? ReceiverIdentityCard { get; set; }
        public int? InstallmentDurationMonth { get; set; }

        public ICollection<OrderItemDetailResponse> Items { get; set; } = [];
        public ICollection<InstallmentResponse> Installments { get; set; } = [];
        public ICollection<PaymentResponse> Payments { get; set; } = [];
    }
}

