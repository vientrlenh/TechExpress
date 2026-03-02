using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;
using TechExpress.Service.Utils;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public ReviewController(ServiceProviders serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Lấy danh sách đánh giá của sản phẩm (public, có phân trang, lọc, sắp xếp).
        /// </summary>
        [HttpGet("product/{productId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<ReviewResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductReviews(
            [FromRoute] Guid productId,
            [FromQuery] ReviewFilterRequest request,
            CancellationToken ct)
        {
            if (request.Page < 1)
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Page phải lớn hơn 0."
                });

            if (request.PageSize < 1 || request.PageSize > 50)
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "PageSize phải trong khoảng 1 đến 50."
                });

            if (request.Rating.HasValue && (request.Rating < 1 || request.Rating > 5))
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Rating lọc phải từ 1 đến 5."
                });

            var pagination = await _serviceProvider.ReviewService.HandleGetProductReviewsAsync(
                productId,
                request.Page,
                request.PageSize,
                request.Rating,
                request.HasMedia,
                request.SortBy,
                request.SortDirection,
                ct);

            var response = ResponseMapper.MapToReviewResponsePagination(pagination);
            return Ok(ApiResponse<Pagination<ReviewResponse>>.OkResponse(response));
        }

        /// <summary>
        /// Tạo đánh giá cho sản phẩm.
        /// Customer đã đăng nhập: FullName và Phone tự động lấy từ profile nếu không truyền.
        /// Guest chưa đăng nhập: Phone bắt buộc phải truyền.
        /// Comment và Rating luôn bắt buộc.
        /// </summary>
        [HttpPost("product/{productId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateReview(
            [FromRoute] Guid productId,
            [FromBody] CreateReviewRequest request,
            CancellationToken ct)
        {
            var review = await _serviceProvider.ReviewService.HandleCreateReviewAsync(
                productId,
                request.FullName,
                request.Phone,
                request.Comment,
                request.Rating,
                request.MediaUrls,
                ct);

            var response = ResponseMapper.MapToReviewResponse(review);
            return Created(string.Empty, ApiResponse<ReviewResponse>.CreatedResponse(response));
        }

        /// <summary>
        /// Xóa mềm đánh giá của chính mình (chỉ dành cho khách hàng đã đăng nhập).
        /// </summary>
        [HttpDelete("{reviewId:guid}")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteReview(
            [FromRoute] Guid reviewId,
            CancellationToken ct)
        {
            await _serviceProvider.ReviewService.HandleDeleteReviewAsync(reviewId, ct);
            return NoContent();
        }
    }
}
