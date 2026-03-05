using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
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
        List<Guid> appliedProductIds, 
        int? minAppliedQuantity, 
        int? maxUsageCount, 
        int? maxUsagePerUser, 
        DateTimeOffset startDate, 
        DateTimeOffset endDate, 
        bool isStackable)
    {        
        code = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpper();
        if (!string.IsNullOrWhiteSpace(code) && await _unitOfWork.PromotionRepository.ExistsByCodeAsync(code))
        {
            throw new BadRequestException($"Mã khuyến mãi đã tồn tại trong hệ thống: {code}");
        }
        ValidatePromotionRequestedStartDateAndEndDate(startDate, endDate);
        if (maxUsageCount.HasValue && maxUsagePerUser.HasValue && maxUsagePerUser.Value > maxUsageCount.Value)
        {
            throw new BadRequestException($"Giới hạn sử dụng mỗi người dùng: {maxUsagePerUser} không được phép vượt quá tổng giới hạn sử dụng: {maxUsageCount}");
        }

        Promotion promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = description,
            Type = type,
            Scope = scope,
            StartDate = startDate,
            EndDate = endDate,
            MaxUsageCount = maxUsageCount,
            MaxUsagePerUser = maxUsagePerUser,
            IsStackable = isStackable
        };

        await ValidatePromotionValuesFromRequestedType(type, discountValue, maxDiscountValue, freeItemPickCount, freeProductCommands, promotion);
        await ValidatePromotionWithRequestedScope(scope, categoryId, brandId, appliedProductIds, minAppliedQuantity, minOrderValue, promotion);
        await ValidatePromotionRequiredProductsFromRequest(requiredProductCommands, requiredProductLogic, promotion);

        await _unitOfWork.PromotionRepository.AddAsync(promotion);
        await _unitOfWork.SaveChangesAsync();

        Promotion newPromotion = await _unitOfWork.PromotionRepository.FindByIdIncludeRequiredProductsIncludeFreeProductsIncludeAppliedProductsWithSplitQueryAsync(promotion.Id) ?? throw new NotFoundException($"Không tìm thấy khuyến mãi {promotion.Id}");
        return newPromotion;
    }

    private static void ValidatePromotionRequestedStartDateAndEndDate(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (startDate.CompareTo(endDate) >= 0)
        {
            throw new BadRequestException($"Thời gian bắt đầu khuyến mãi {startDate} không được phép ở sau hoặc trùng với ngày kết thúc khuyến mãi: {endDate}");
        }
    }

    private async Task ValidatePromotionValuesFromRequestedType(PromotionType type, decimal? discountValue, decimal? maxDiscountValue, int? freeItemPickCount, List<CreatePromotionFreeProductCommand> freeProductCommands, Promotion promotion)
    {
        switch (type)
        {
            case PromotionType.PercentageDiscount:
                ValidatePromotionValueWithPercentageDiscountType(discountValue);
                freeItemPickCount = null;
                break;
            case PromotionType.FixedDiscount:
                ValidatePromotionValueWithFixedDiscountType(discountValue);
                freeItemPickCount = null;
                maxDiscountValue = null;
                break;
            case PromotionType.FreeItem:
                await ValidateFreeItemFromRequest(freeProductCommands, freeItemPickCount, promotion);
                discountValue = null;
                maxDiscountValue = null;
                break;
            default:
                throw new BadRequestException($"Khuyến mãi hiện tại không hỗ trợ loại {type}");
        }
        promotion.DiscountValue = discountValue;
        promotion.MaxDiscountValue = maxDiscountValue;
        promotion.FreeItemPickCount = freeItemPickCount;
    }

    private static void ValidatePromotionValueWithPercentageDiscountType(decimal? discountValue)
    {
        if (!discountValue.HasValue)
        {
            throw new BadRequestException($"Khuyến mãi kiểu giảm theo % yêu cầu giá trị được giảm");
        }
        if (discountValue.Value >= 100)
        {
            throw new BadRequestException($"Giá trị giảm của khuyến mãi % không được phép vượt quá hoặc bằng 100");
        }
    }

    private static void ValidatePromotionValueWithFixedDiscountType(decimal? discountValue)
    {
        if (!discountValue.HasValue)
        {
            throw new BadRequestException($"Khuyến mãi kiểu giảm theo giá cứng yêu cầu giá trị được giảm");
        }
    }

    private async Task ValidateFreeItemFromRequest(List<CreatePromotionFreeProductCommand> freeProductCommands, int? freeItemPickCount, Promotion promotion)
    {
        if (freeProductCommands.Count == 0)
        {
            throw new BadRequestException($"Khuyến mãi kiểu tặng sản phẩm cần phải được liệt kê danh sách sản phẩm tặng");
        }
        if (freeItemPickCount.HasValue && freeItemPickCount.Value > freeProductCommands.Count)
        {
            throw new BadRequestException($"Số lượng sản phẩm được chọn ({freeItemPickCount}) không được phép vượt quá số lượng sản phẩm tặng trong danh sách ({freeProductCommands.Count})");
        }
        List<Guid> requestedFreeItemIds = freeProductCommands.Select(p => p.ProductId).ToList();
        List<Product> freeProducts = await _unitOfWork.ProductRepository.FindByIdsAndActiveAsync(requestedFreeItemIds);
        List<Guid> missingItemIds = requestedFreeItemIds.Except(freeProducts.Select(p => p.Id)).ToList();
        if (missingItemIds.Count > 0)
        {
            string missingIds = string.Join(", ", missingItemIds);
            throw new NotFoundException($"Các sản phẩm: {missingIds} không tìm thấy hoặc không còn kinh doanh để làm sản phẩm quà tặng");
        }
        foreach (var command in freeProductCommands)
        {
            promotion.FreeProducts.Add(new PromotionFreeProduct
            {
                ProductId = command.ProductId,
                Quantity = command.Quantity
            });
        }
    }

    private async Task ValidatePromotionWithRequestedScope(PromotionScope scope, Guid? categoryId, Guid? brandId, List<Guid> appliedProductIds, int? minAppliedQuantity, decimal? minOrderValue, Promotion promotion)
    {
        switch (scope)
        {
            case PromotionScope.Order:
                categoryId = null;
                brandId = null;
                minAppliedQuantity = null;
                break;
            case PromotionScope.Product:
                await ValidatePromotionProductScopeFromRequest(appliedProductIds, promotion);
                minOrderValue = null;
                categoryId = null;
                brandId = null;
                break;
            case PromotionScope.Category:
                await ValidatePromotionCategoryScopeFromRequest(categoryId);
                minOrderValue = null;
                brandId = null;
                break;
            case PromotionScope.Brand:
                await ValidatePromotionBrandScopeFromRequest(brandId);
                minOrderValue = null;
                categoryId = null;
                break;
            default:
                throw new BadRequestException($"Khuyến mãi hiện tại không hỗ trợ scope {scope}");
        }
        promotion.MinOrderValue = minOrderValue;
        promotion.CategoryId = categoryId;
        promotion.BrandId = brandId;
        promotion.MinAppliedQuantity = minAppliedQuantity;
    }


    private async Task ValidatePromotionProductScopeFromRequest(List<Guid> appliedProductIds, Promotion promotion)
    {
        if (appliedProductIds.Count == 0)
        {
            throw new BadRequestException("Các sản phẩm áp dụng khuyến mãi không được để trống cho scope sản phẩm");
        }
        appliedProductIds = appliedProductIds.Distinct().ToList();
        List<Product> appliedProducts = await _unitOfWork.ProductRepository.FindByIdsAndActiveAsync(appliedProductIds);
        List<Guid> missingProductIds = appliedProductIds.Except(appliedProducts.Select(p => p.Id).ToList()).ToList();
        if (missingProductIds.Count > 0)
        {
            string missingIds = string.Join(", ", missingProductIds);
            throw new BadRequestException($"Không tìm thấy các sản phẩm: {missingIds} hoặc không còn kinh doanh để làm sản phẩm áp dụng khuyến mãi");
        }
        foreach (var appliedProductId in appliedProductIds)
        {
            promotion.AppliedProducts.Add(new PromotionAppliedProduct
            {
                ProductId = appliedProductId
            });
        }
    }

    private async Task ValidatePromotionCategoryScopeFromRequest(Guid? categoryId)
    {
        if (!categoryId.HasValue)
        {
            throw new BadRequestException("Khuyến mãi trên danh mục phải nhập danh mục cần được áp dụng");
        }
        if (!await _unitOfWork.CategoryRepository.ExistByIdAndIsNotDeleted(categoryId.Value))
        {
            throw new NotFoundException($"Không tìm thấy danh mục {categoryId.Value} hoặc danh mục đã bị xóa khỏi hệ thống");
        }
    }

    private async Task ValidatePromotionBrandScopeFromRequest(Guid? brandId)
    {
        if (!brandId.HasValue)
        {
            throw new BadRequestException("Khuyến mãi trên nhà cung cấp phải nhập nhà cung cấp cần được áp dụng");
        }
        if (!await _unitOfWork.BrandRepository.ExistsByIdAsync(brandId.Value))
        {
            throw new NotFoundException($"Không tìm thấy nhà cung cấp {brandId.Value} trong hệ thống");
        }
    }

    private async Task ValidatePromotionRequiredProductsFromRequest(List<CreatePromotionRequiredProductCommand> requiredProductCommands, PromotionRequiredProductLogic? requiredProductLogic, Promotion promotion)
    {
        if (requiredProductCommands.Count > 0)
        {
            if (requiredProductCommands.Count > 1 && requiredProductLogic is null)
            {
                throw new BadRequestException("Logic cho sản phẩm yêu cầu trong khuyến mãi bắt buộc phải có khi có nhiều hơn 1 sản phẩm yêu cầu");
            }
            List<Guid> requestRequiredProductIds = requiredProductCommands.Select(p => p.ProductId).ToList();
            List<Product> requestRequiredProducts = await _unitOfWork.ProductRepository.FindByIdsAndActiveAsync(requestRequiredProductIds);
            List<Guid> missingRequiredProductIds = requestRequiredProductIds.Except(requestRequiredProducts.Select(p => p.Id).ToList()).ToList();
            if (missingRequiredProductIds.Count > 0)
            {
                string missingIds = string.Join(", ", missingRequiredProductIds);
                throw new BadRequestException($"Các sản phẩm yêu cầu cho khuyến mãi không tồn tại hoặc không còn kinh doanh: {missingIds}");
            }
            foreach (var command in requiredProductCommands)
            {
                if (command.MinQuantity <= 0)
                {
                    throw new BadRequestException($"Sản phẩm yêu cầu tối thiểu cho khuyến mãi: {command.ProductId} không được phép có số lượng dưới hoặc bằng 0");
                }
                if (command.MaxQuantity.HasValue && command.MaxQuantity.Value <= command.MinQuantity)
                {
                    throw new BadRequestException($"Sản phẩm yêu cầu tối đa: {command.ProductId} cho khuyến mãi bắt buộc phải lớn hơn sản phẩm tối thiểu nếu có");
                }
                promotion.RequiredProducts.Add(new PromotionRequiredProduct
                {
                    ProductId = command.ProductId,
                    MinQuantity = command.MinQuantity,
                    MaxQuantity = command.MaxQuantity
                });
            }
        }
        else
        {
            requiredProductLogic = null;
        }
        promotion.RequiredProductLogic = requiredProductLogic;
        
    }
}
