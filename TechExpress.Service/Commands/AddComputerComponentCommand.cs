using System;

namespace TechExpress.Service.Commands;

public class AddComputerComponentCommand
{
    public required Guid ComponentId { get; set; }

    public required int Quantity { get; set; }
}
