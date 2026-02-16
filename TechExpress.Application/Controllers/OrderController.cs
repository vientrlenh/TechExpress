using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExpress.Application.Common;
using TechExpress.Application.DTOs.Requests;
using TechExpress.Application.DTOs.Responses;
using TechExpress.Service;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public OrderController(ServiceProviders serviceProviders)
        {
            _serviceProvider = serviceProviders;
        }

        /// <summary>
        /// Mua hàng không cần tài khoản (Guest Checkout)
        /// </summary>
        [HttpPost("guest-checkout")]
        public async Task<IActionResult> GuestCheckout([FromBody] GuestCheckoutRequest request)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Value = "API này chỉ dành cho khách chưa đăng nhập."
                });
            }

            try
            {
                var items = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();

                // Nhận kết quả dạng Tuple (order, danh sách installments)
                var (order, installments) = await _serviceProvider.OrderService.HandleGuestCheckoutAsync(
                    items,
                    request.DeliveryType,
                    request.ReceiverEmail,
                    request.ReceiverFullName,
                    request.ShippingAddress,
                    request.TrackingPhone,
                    request.PaidType,
                    request.ReceiverIdentityCard,
                    request.InstallmentDurationMonth,
                    request.Notes
                );

                var response = ResponseMapper.MapToOrderResponseFromOrder(order, installments);
                return Ok(ApiResponse<OrderResponse>.OkResponse(response));
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new ApiResponse<string>
                {
                    StatusCode = 409,
                    Value = "Hệ thống đang bận do có nhiều người mua cùng lúc, vui lòng thử lại sau giây lát."
                });
            }
        }

        /// <summary>
        /// khách hàng đăng nhập mua product (Member Checkout)
        /// </summary>
        [HttpPost("member-checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MemberCheckout([FromBody] MemberCheckoutRequest request)
        {
            try
            {
                var userId = _serviceProvider.UserContext.GetCurrentAuthenticatedUserId();

                // Nhận kết quả dạng Tuple (order, danh sách installments)
                var (order, installments) = await _serviceProvider.OrderService.HandleMemberCheckoutAsync(
                    userId,
                    request.SelectedCartItemIds,
                    request.DeliveryType,
                    request.ReceiverEmail,
                    request.ReceiverFullName,
                    request.ShippingAddress,
                    request.TrackingPhone,
                    request.PaidType,
                    request.ReceiverIdentityCard,
                    request.InstallmentDurationMonth,
                    request.Notes
                );

                var response = ResponseMapper.MapToOrderResponseFromOrder(order, installments);
                return Ok(ApiResponse<OrderResponse>.OkResponse(response));
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new ApiResponse<string>
                {
                    StatusCode = 409,
                    Value = "Hàng trong giỏ của bạn vừa có người khác mua mất, vui lòng kiểm tra lại số lượng tồn kho."
                });
            }
        }
    }
}