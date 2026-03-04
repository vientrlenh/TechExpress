using System;
using TechExpress.Repository;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;

namespace TechExpress.Service.Services;

public class PromotionService
{
    private readonly UnitOfWork _unitOfWork;

    public PromotionService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Promotion> HandleCreatePromotion(
        string name, 
        string? code, 
        string description, 
        PromotionType type, 
        PromotionScope scope, 
        decimal? discountValue, 
        decimal? maxDiscountValue,
        decimal? minOrderValue, 
        List<CreatePromotionRequiredProductCommand> requiredProductCommands, 
        PromotionRequiredProductLogic? requiredProductLogic, 
        List<CreatePromotionFreeProductCommand> freeProductCommands, 
        int? freeItemPickCount, 
        Guid? categoryId, 
        Guid? brandId, 
        List<Guid> appliedProducts, 
        int? minAppliedQuantity, 
        int? maxUsageCount, 
        int usageCount, 
        int? maxUsagePerUser, 
        string startDateStr, 
        string endDateStr, 
        bool isStackable)
    {        
        return null;
    }
}
