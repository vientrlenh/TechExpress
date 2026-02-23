using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
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

        /// <summary>
        /// Query: danh sách Payment theo Order.
        /// </summary>
        [HttpGet("{orderId:guid}/payments")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PaymentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderPayments([FromRoute] Guid orderId, CancellationToken ct)
        {
            var payments = await _serviceProvider.PaymentService
                .HandleGetPaymentsByOrderAsync(orderId, ct);

            var response = ResponseMapper.MapToPaymentResponseList(payments);

            return Ok(ApiResponse<List<PaymentResponse>>.OkResponse(response));
        }

        /// <summary>
        /// Query: lịch trả góp (Installment schedule) theo Order.
        /// </summary>
        [HttpGet("{orderId:guid}/installment")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<InstallmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderInstallments([FromRoute] Guid orderId, CancellationToken ct)
        {
            var schedule = await _serviceProvider.InstallmentService
                .HandleGetInstallmentScheduleByOrderAsync(orderId, ct);

            var response = ResponseMapper.MapToInstallmentResponseListFromInstallmentList(schedule);

            return Ok(ApiResponse<List<InstallmentResponse>>.OkResponse(response));
        }

        /// <summary>
        /// Checkout: chọn trả thẳng (Full) + payment method.
        /// </summary>
        /// <remarks>
        /// <para><b>Request</b></para>
        /// <code>{ "method": 1 }</code>
        ///
        /// <para><b>Response</b></para>
        /// <code>
        /// {
        ///   "statusCode": 200,
        ///   "message": "OK",
        ///   "value": { "orderId": "GUID", "paidType": "Full", "method": "PayOs" }
        /// }
        /// </code>
        /// </remarks>
        [HttpPut("{orderId:guid}/payment-intent")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SetPaymentIntentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPaymentIntent(
            [FromRoute] Guid orderId,
            [FromBody] SetPaymentIntentRequest request,
            CancellationToken ct)
        {
            if (request == null)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Request body is required."
                });
            }

            // Service trả về Order (hoặc model khác). Controller chỉ map ra response DTO.
            var order = await _serviceProvider.PaymentService
                .HandleSetFullPaymentIntentAsync(orderId, request.Method, ct);

            var response = ResponseMapper.MapToSetPaymentIntentResponse(order.Id, request.Method);

            return Ok(ApiResponse<SetPaymentIntentResponse>.OkResponse(response));
        }

        /// <summary>
        /// Checkout: chọn trả góp (Installment) + tạo schedule theo kỳ.
        /// </summary>
        [HttpPut("{orderId:guid}/installment-intent")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SetInstallmentIntentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetInstallmentIntent(
            [FromRoute] Guid orderId,
            [FromBody] SetInstallmentIntentRequest request,
            CancellationToken ct)
        {
            var schedule = await _serviceProvider.InstallmentService
    .HandleCreateInstallmentScheduleAsync(orderId, request.Months, ct);

            if (schedule == null || schedule.Count == 0)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Không tạo được lịch trả góp."
                });
            }

            var response = ResponseMapper.MapToSetInstallmentIntentResponse(
                orderId,
                request.Months,
                schedule);

            return Ok(ApiResponse<SetInstallmentIntentResponse>.OkResponse(response));

        }
    }
}