using System;

namespace TechExpress.Service.Commands;

public class CheckoutItemCommand
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? BrandId { get; set; }
}
