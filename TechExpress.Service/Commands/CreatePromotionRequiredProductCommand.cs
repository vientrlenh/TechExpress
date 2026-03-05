using System;

namespace TechExpress.Service.Commands;

public class CreatePromotionRequiredProductCommand
{
    public Guid ProductId { get; set; }
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
}
