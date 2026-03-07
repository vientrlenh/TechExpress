namespace TechExpress.Repository.Models;

public class OrderItem
{
    public long Id { get; set; }

    public required Guid OrderId { get; set; }

    public required Guid ProductId { get; set; }

    public required int Quantity { get; set; }

    public required decimal UnitPrice { get; set; }

    public bool IsFreeItem { get; set; }

    public int WarrantyMonthSnapshot { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
