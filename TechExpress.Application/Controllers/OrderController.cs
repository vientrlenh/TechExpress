using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        /// Buy hàng không cần tài khoản (Guest Checkout)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost("guest-checkout")]
        public async Task<IActionResult> GuestCheckout([FromBody] GuestCheckoutRequest request)
        {
            var items = request.Items
                .Select(i => (i.ProductId, i.Quantity))
                .ToList();

            var order = await _serviceProvider.OrderService.HandleGuestCheckoutAsync(
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

            // Chuyển đổi sang Response DTO trước khi trả về
            var response = ResponseMapper.MapToOrderResponseFromOrder(order);

            return Ok(ApiResponse<OrderResponse>.OkResponse(response));
        }

        /// <summary>
        /// khách hàng đăng nhập mua product (Member Checkout)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost("member-checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MemberCheckout([FromBody] MemberCheckoutRequest request)
        {
            // Lấy UserId từ Token đang đăng nhập
            var userId = _serviceProvider.UserContext.GetCurrentAuthenticatedUserId();

            // Truyền từng thuộc tính vào Service (Bao gồm SelectedCartItemIds)
            var order = await _serviceProvider.OrderService.HandleMemberCheckoutAsync(
                userId,
                request.SelectedCartItemIds, // Truyền List ID
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

            var response = ResponseMapper.MapToOrderResponseFromOrder(order);
            return Ok(ApiResponse<OrderResponse>.OkResponse(response));
        }
    }
}