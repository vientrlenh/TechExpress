using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Responses;

public record PromotionRequiredProductResponse(
    long Id,
    Guid ProductId,
    int MinQuantity,
    int? MaxQuantity
);

public record PromotionFreeProductResponse(
    long Id,
    Guid ProductId,
    int Quantity
);

public record PromotionAppliedProductResponse(
    long Id,
    Guid ProductId
);

public record PromotionResponse(
    Guid Id,
    string Name,
    string? Code,
    string Description,
    PromotionType Type,
    PromotionScope Scope,
    decimal? DiscountValue,
    decimal? MaxDiscountValue,
    decimal? MinOrderValue,
    List<PromotionRequiredProductResponse> RequiredProducts,
    PromotionRequiredProductLogic? RequiredProductLogic,
    List<PromotionFreeProductResponse> FreeProducts,
    int? FreeItemPickCount,
    Guid? CategoryId,
    Guid? BrandId,
    List<PromotionAppliedProductResponse> AppliedProducts,
    int? MinAppliedQuantity,
    int? MaxUsageCount,
    int UsageCount,
    int? MaxUsagePerUser,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool IsStackable,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
