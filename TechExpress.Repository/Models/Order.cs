using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class Order
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];

    public required DeliveryType DeliveryType { get; set; }

    public required decimal SubTotal { get; set; }

    public required decimal ShippingCost { get; set; }

    public required decimal Tax { get; set; }

    public required decimal TotalPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public string? ReceiverEmail { get; set; }

    public string? ReceiverFullName { get; set; }

    public string? ShippingAddress { get; set; }

    public required string TrackingPhone { get; set; }

    public string? Notes { get; set; }

    public required PaidType PaidType { get; set; }

    public string? ReceiverIdentityCard { get; set; }

    public int? InstallmentDurationMonth { get; set; }

    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;

    public Guid? DeliveredById { get; set; }

    public Guid? CreatedByStaffId { get; set; }

    public string? CourierService { get; set; }

    public string? CourierTrackingCode { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public DateTimeOffset? ReceivedAt { get; set; }

    public required OrderStatus Status { get; set; }

    public User? User { get; set; }

    public User? DeliveredBy { get; set; }

    public User? CreatedByStaff { get; set; }
}
