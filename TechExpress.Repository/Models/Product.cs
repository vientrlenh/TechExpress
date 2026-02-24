using System;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class Product
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Sku { get; set; }

    public required Guid CategoryId { get; set; }

    public Guid? BrandId { get; set; }

    public required decimal Price { get; set; }

    public required int Stock { get; set; }

    public required string Description { get; set; }

    public ICollection<ProductSpecValue> SpecValues { get; set; } = [];

    public required int WarrantyMonth { get; set; }

    public required ProductStatus Status { get; set; }

    public ICollection<ProductImage> Images { get; set; } = [];

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now; 

    public Category Category { get; set; } = null!;

    public Brand? Brand { get; set; }
}
