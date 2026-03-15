using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Application.DTOs.Responses;
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
        /// List promotion dành cho Admin (Xem được tất cả trạng thái)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPromotionsForAdmin(
            [FromQuery] string? search,
            [FromQuery] bool? status, // Admin lọc true/false tùy ý
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var entityPagination = await _serviceProvider.PromotionService.GetPromotionsPagedAsync(
                search, status, fromDate, toDate, sortBy, isDescending, page, pageSize);

            var response = ResponseMapper.MapToPromotionListResponsePaginationFromPromotionPagination(entityPagination);
            return Ok(ApiResponse<Pagination<PromotionListResponse>>.OkResponse(response));
        }


        /// <summary>
        /// List promotion dành cho Khách hàng (Chỉ hiện mã đang active và còn hạn)
        /// </summary>
        [HttpGet("customer_guest/all")]
        public async Task<IActionResult> GetAllPromotionsForCustomer(
            [FromQuery] string? search,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Ở đây KHÔNG NHẬN tham số status từ client để đảm bảo bảo mật
            var entityPagination = await _serviceProvider.PromotionService.GetPromotionsPagedForCustomerGuestAsync(
                search, fromDate, toDate, sortBy, isDescending, page, pageSize);

            var response = ResponseMapper.MapToPromotionListResponsePaginationFromPromotionPagination(entityPagination);
            return Ok(ApiResponse<Pagination<PromotionListResponse>>.OkResponse(response));
        }



        [HttpPost("disable")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisablePromotion([FromBody] DisablePromotionRequest request)
        {
            var promotion = await _serviceProvider.PromotionService.HandleDisablePromotion(request.PromotionId);
            var response = ResponseMapper.MapToPromotionResponseFromPromotion(promotion);
            return Ok(ApiResponse<PromotionResponse>.OkResponse(response));
        }

        [HttpDelete("{promotionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePromotion(Guid promotionId)
        {
            await _serviceProvider.PromotionService.HandleDeletePromotion(promotionId);
            return Ok(ApiResponse<string>.OkResponse("Xóa khuyến mãi thành công."));
        }


        [HttpGet("{promotionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPromotionDetail(Guid promotionId)
        {
            var promotion = await _serviceProvider.PromotionService.HandleGetPromotionDetail(promotionId);
            var response = ResponseMapper.MapToPromotionDetailResponseFromPromotion(promotion);

            return Ok(ApiResponse<PromotionDetailResponse>.OkResponse(response));
        }

        /// <summary>
        /// cho guest hoặc customer lấy thông tin bằng promotion code
        /// </summary>
        /// <param name="promotionCode"></param>
        /// <returns></returns>
        [HttpGet("{promotionCode}/customer/guest")]
        public async Task<IActionResult> GetPromotionDetailCode(string promotionCode)
        {
            var promotion = await _serviceProvider.PromotionService.HandleGetPromotionCodeDetail(promotionCode);
            var response = ResponseMapper.MapToPromotionDetailResponseFromPromotion(promotion);

            return Ok(ApiResponse<PromotionDetailResponse>.OkResponse(response));
        }


    }
}