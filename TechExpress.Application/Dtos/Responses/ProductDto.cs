using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Responses
{
    public record ProductListResponse(
        Guid Id,
        string Name,
        string Sku,
        Guid CategoryId,
        Guid? BrandId,
        string CategoryName,
        decimal Price,
        int Stock,
        int WarrantyMonth,
        ProductStatus Status,
        string? FirstImageUrl,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
    );

    public record ProductSpecValueResponse(
        Guid SpecDefinitionId,
        string SpecName,
        string Unit,
        string Code,
        SpecAcceptValueType DataType,
        string Value
    );

    public record ProductDetailResponse(
        Guid Id,
        string Name,
        string Sku,
        Guid CategoryId,
        Guid? BrandId,
        string CategoryName,
        decimal Price,
        int Stock,
        int WarrantyMonth,
        ProductStatus Status,
        string Description,
        List<string>? ThumbnailUrl,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        List<ProductSpecValueResponse> SpecValues
    );

    public record ProductPCComponentResponse(
        Guid ComponentProductId,
        string ComponentProductName,
        string ComponentProductSku,
        int Quantity
    );

    public record ProductPCDetailResponse(
        Guid Id,
        string Name,
        string Sku,
        Guid CategoryId,
        Guid? BrandId,
        string CategoryName,
        decimal Price,
        int Stock,
        int WarrantyMonth,
        ProductStatus Status,
        string Description,
        List<string>? ThumbnailUrl,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        List<ProductSpecValueResponse> SpecValues,
        List<ProductPCComponentResponse> Components
    );
}
