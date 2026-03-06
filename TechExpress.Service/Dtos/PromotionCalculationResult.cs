using System;

namespace TechExpress.Service.Dtos;

public record FreeItemResult(
    Guid ProductId, 
    int Quantity
);

public record PromotionLineResult(
    Guid PromotionId,
    string PromotionName,
    string? PromotionCode,
    decimal DiscountAmount,
    List<FreeItemResult> FreeItems,
    int? FreeItemPickCount
);

public record PromotionCalculationResult(
    List<PromotionLineResult> AppliedPromotions,
    decimal TotalDiscountAmount,
    List<FreeItemResult> TotalFreeItems,
    List<string> UnappliedCodeMessages
);