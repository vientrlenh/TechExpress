using System;

namespace TechExpress.Service.Commands;

public class CreatePromotionFreeProductCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
