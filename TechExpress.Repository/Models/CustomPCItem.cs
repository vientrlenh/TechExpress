using System;

namespace TechExpress.Repository.Models;

public class CustomPCItem
{
    public long Id { get; set; }

    public Guid CustomPCId { get; set; }

    public Guid ProductId { get; set; }

    public required int Quantity { get; set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

    public CustomPC CustomPC { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
