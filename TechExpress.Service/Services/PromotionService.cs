using System;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;
using TechExpress.Service.Utils;

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



    // 1. Dành cho Admin: Gọi hàm xử lý chung với cờ checkActive = false
    public Task<Pagination<Product>> GetPromotionProductsForAdminAsync(Guid id, string? search, int page, int pageSize)
        => GetPromotionProductsInternalAsync(id, search, page, pageSize, checkActive: false);

    // 2. Dành cho Khách hàng: Gọi hàm xử lý chung với cờ checkActive = true
    public Task<Pagination<Product>> GetPromotionProductsForCustomerAsync(Guid id, string? search, int page, int pageSize)
        => GetPromotionProductsInternalAsync(id, search, page, pageSize, checkActive: true);

    // 3. Hàm xử lý logic lõi (Core Logic)
    private async Task<Pagination<Product>> GetPromotionProductsInternalAsync(
        Guid id,
        string? search,
        int page,
        int pageSize,
        bool checkActive)
    {
        // Tối ưu: Lấy thông tin KM một lần duy nhất.
        // Bạn nên đảm bảo hàm FindByIdIncludeAppliedProductsAsync trong Repository sử dụng .AsNoTracking() để tăng tốc độ truy vấn.
        var promotion = await _unitOfWork.PromotionRepository.FindByIdIncludeAppliedProductsAsync(id)
            ?? throw new NotFoundException("Không tìm thấy chương trình khuyến mãi.");

        // Tối ưu hiệu suất: Kiểm tra điều kiện ngắt sớm cho Customer
        if (checkActive)
        {
            var now = DateTimeOffset.Now;
            if (now < promotion.StartDate || now > promotion.EndDate)
            {
                throw new BadRequestException("Khuyến mãi này hiện không khả dụng hoặc đã hết hạn.");
            }
        }

        // Tối ưu bộ nhớ: Trích xuất ID trực tiếp từ Navigation Property
        var appliedIds = promotion.AppliedProducts.Select(ap => ap.ProductId).ToList();

        // Tối ưu: Giải nén Tuple từ Repository một cách trực quan
        var (products, totalCount) = await _unitOfWork.ProductRepository.GetProductsByPromotionScopeAsync(
            promotion.Scope,
            promotion.CategoryId,
            promotion.BrandId,
            appliedIds,
            search,
            page,
            pageSize);

        // Trả về object phân trang đồng nhất
        return new Pagination<Product>
        {
            Items = products,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    // ===================== List Promotion ==================================
    // TechExpress.Service/Services/PromotionService.cs

    public async Task<Pagination<Promotion>> GetPromotionsPagedAsync(
        string? search,
        string? status,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        string sortBy,
        bool isDescending,
        int page,
        int pageSize)
    {
        var (promotions, totalCount) = await _unitOfWork.PromotionRepository.FindPromotionsPagedAsync(
            search, status, fromDate, toDate, sortBy, isDescending, page, pageSize);

        return new Pagination<Promotion>
        {
            Items = promotions,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}