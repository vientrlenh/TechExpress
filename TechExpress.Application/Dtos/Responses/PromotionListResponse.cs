namespace TechExpress.Application.DTOs.Responses;

public record PromotionListResponse(
    Guid Id,
    string Name,
    string? Code,
    string Description,
    string Type,
    string Scope,
    decimal? DiscountValue,
    int UsageCount,
    int? MaxUsageCount,
    bool IsActive,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    DateTimeOffset CreatedAt,
    bool IsExpired
);