namespace TechExpress.Application.Dtos.Responses;

public record PromotionListResponse(
    Guid Id,
    string Name,
    string? Code,
    string Description,
    string Type,
    string Scope,
    decimal? DiscountValue,
    decimal? MaxDiscountValue,
    decimal? MinOrderValue,
    int UsageCount,
    int? MaxUsageCount,
    int? MaxUsagePerUser,
    bool IsActive,
    bool IsStackable,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    DateTimeOffset CreatedAt,
    bool IsExpired
);