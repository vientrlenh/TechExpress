using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;
using TechExpress.Service.Contexts;
using TechExpress.Service.Dtos;
using TechExpress.Service.Utils;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;
        private readonly UserContext _userContext;

        public PromotionController(ServiceProviders serviceProvider, UserContext userContext)
        {
            _serviceProvider = serviceProvider;
            _userContext = userContext;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            var startDate = DateMapper.ConvertToDateTimeOffsetFromString(request.StartDate);
            var endDate = DateMapper.ConvertToDateTimeOffsetFromString(request.EndDate);
            var requiredProductCommands = RequestMapper.MapToCreatePromotionRequiredProductCommandListFromRequest(request.RequiredProducts);
            var freeProductCommands = RequestMapper.MapToCreatePromotionFreeProductCommandListFromRequest(request.FreeProducts);

            var promotion = await _serviceProvider.PromotionService.HandleCreatePromotion(
                request.Name.Trim(),
                request.Code,
                request.Description.Trim(),
                request.Type,
                request.Scope,
                request.DiscountValue,
                request.MaxDiscountValue,
                request.MinOrderValue,
                requiredProductCommands,
                request.RequiredProductLogic,
                freeProductCommands,
                request.FreeItemPickCount,
                request.CategoryId,
                request.BrandId,
                request.AppliedProducts,
                request.MinAppliedQuantity,
                request.MaxUsageCount,
                request.MaxUsagePerUser,
                startDate,
                endDate,
                request.IsStackable);

            var response = ResponseMapper.MapToPromotionResponseFromPromotion(promotion);
            return CreatedAtAction(nameof(CreatePromotion), ApiResponse<PromotionResponse>.CreatedResponse(response));
        }

        [HttpPost("calculate")]
        [AllowAnonymous]
        public async Task<IActionResult> CalculatePromotion([FromBody] CalculatePromotionRequest request)
        {
            Guid? userId = null;
            string? userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
            if (userIdStr != null) userId = Guid.Parse(userIdStr);

            var checkoutItemCommands = RequestMapper.MapToCheckoutItemCommandListFromRequest(request.CheckoutItems);
            var result = await _serviceProvider.PromotionService.CalculateForCheckoutAsync(
                request.Codes, checkoutItemCommands, userId, request.Phone);

            return Ok(ApiResponse<PromotionCalculationResult>.OkResponse(result));
        }

        // ======================== ADMIN VIEW API ========================
        /// <summary>
        /// Dành cho Admin xem sản phẩm của bất kỳ KM nào (kể cả Inactive/Expired)
        /// </summary>
        /// <param name="promotionId"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("admin/{promotionId}/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPromotionProductsAdmin(
            Guid promotionId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // 1. Gọi Service lấy dữ liệu phân trang (Không check IsActive)
            var pagination = await _serviceProvider.PromotionService.GetPromotionProductsForAdminAsync(promotionId, search, page, pageSize);

            // 2. Sử dụng ResponseMapper bạn đã có để map sang ProductListResponse
            var response = ResponseMapper.MapToProductListResponsePaginationFromProductPagination(pagination);

            return Ok(ApiResponse<Pagination<ProductListResponse>>.OkResponse(response));
        }

        // ======================== CUSTOMER VIEW API ========================
        /// <summary>
        /// Dành cho khách hàng xem sản phẩm của các KM đang hoạt động
        /// </summary>
        /// <param name="promotionId"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("customer/{promotionId}/products")]
        [AllowAnonymous] // Hoặc [Authorize(Roles = "Customer")] tùy chính sách bảo mật của bạn
        public async Task<IActionResult> GetPromotionProductsPublic(
            Guid promotionId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // 1. Gọi Service lấy dữ liệu (Có check IsActive và thời gian hiệu lực)
            var pagination = await _serviceProvider.PromotionService.GetPromotionProductsForCustomerAsync(promotionId, search, page, pageSize);

            // 2. Map dữ liệu sang DTO để hiển thị trên UI người dùng
            var response = ResponseMapper.MapToProductListResponsePaginationFromProductPagination(pagination);

            return Ok(ApiResponse<Pagination<ProductListResponse>>.OkResponse(response));
        }
        /// <summary>
        /// List tất cả promotion cho admin
        /// </summary>
        /// <param name="search"></param>
        /// <param name="status"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortBy"></param>
        /// <param name="isDescending"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPromotions(
            [FromQuery] string? search,
            [FromQuery] string? status, // Nhận filter status
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            [FromQuery] string sortBy = "Code or Name",
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // 1. Service lấy Entity
            var entityPagination = await _serviceProvider.PromotionService.GetPromotionsPagedAsync(
                search, status, fromDate, toDate, sortBy, isDescending, page, pageSize);

            // 2. Map sang DTO để trả về attribute IsExpired và Status chuỗi
            var response = ResponseMapper.MapToPromotionListResponsePaginationFromPromotionPagination(entityPagination);

            return Ok(ApiResponse<Pagination<PromotionListResponse>>.OkResponse(response));
        }

        [HttpPost("{promotionId}/disable")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisablePromotion([FromRoute] Guid promotionId)
        {
            var promotion = await _serviceProvider.PromotionService.HandleDisablePromotion(promotionId);
            var response = ResponseMapper.MapToPromotionResponseFromPromotion(promotion);
            return Ok(ApiResponse<PromotionResponse>.OkResponse(response));
        }
    }
}