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
using TechExpress.Service.Contexts;
using TechExpress.Service.Utils;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;
        private readonly UserContext _userContext;

        public OrderController(ServiceProviders serviceProviders, UserContext userContext)
        {
            _serviceProvider = serviceProviders;
            _userContext = userContext;
        }

        /// <summary>
        /// Danh sách OrderStatus để FE render combo box filter.
        /// </summary>
        [HttpGet("status-options")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<EnumOptionResponse>>), StatusCodes.Status200OK)]
        public IActionResult GetOrderStatusOptions()
        {
            var values = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new EnumOptionResponse
                {
                    Value = (int)s,
                    Name = s.ToString()
                })
                .ToList();

            return Ok(ApiResponse<List<EnumOptionResponse>>.OkResponse(values));
        }

        /// <summary>
        /// Lấy danh sách đơn hàng với search, filter và sort
        /// </summary>
        [HttpGet]

        [ProducesResponseType(typeof(ApiResponse<Pagination<OrderListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrderList([FromQuery] OrderFilterRequest request)
        {
            if (request.Page < 1)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Page must be greater than 0"
                });
            }

            var orderPagination = await _serviceProvider.OrderService
                .HandleGetOrderListWithPaginationAsync(
                    request.Page,
                    request.PageSize,
                    request.SortBy,
                    request.SortDirection,
                    request.Search,
                    request.Status
                );

            var response = ResponseMapper
                .MapToOrderListResponsePaginationFromOrderPagination(orderPagination);

            return Ok(ApiResponse<Pagination<OrderListItemResponse>>.OkResponse(response));
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của khách hàng hiện tại với search tìm item trong đơn hàng , filter và sort
        /// </summary>
        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<OrderListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOrderList([FromQuery] OrderFilterRequest request)
        {
            if (request.Page < 1)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Page must be greater than 0"
                });
            }

            var customerId = _userContext.GetCurrentAuthenticatedUserId();

            var orderPagination = await _serviceProvider.OrderService
                .HandleGetCustomerOrderListWithPaginationAsync(
                    customerId,
                    request.Page,
                    request.PageSize,
                    request.SortBy,
                    request.SortDirection,
                    request.Search,
                    request.Status
                );

            var response = ResponseMapper
                .MapToOrderListResponsePaginationFromOrderPagination(orderPagination);

            return Ok(ApiResponse<Pagination<OrderListItemResponse>>.OkResponse(response));
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

                // Nhận kết quả Tuple 3 (order, installments, usages) từ Service
                var (order, installments, usages) = await _serviceProvider.OrderService.HandleGuestCheckoutAsync(
                    items,
                    request.PromotionCodes,        // Danh sách mã KM khách nhập
                    request.ChosenFreeProductIds, // Danh sách quà tặng khách chọn
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

                // Truyền đủ 3 tham số vào ResponseMapper
                var response = ResponseMapper.MapToOrderResponseFromOrder(order, installments, usages);

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
        /// Khách hàng đăng nhập mua product (Member Checkout)
        /// </summary>
        [HttpPost("member-checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MemberCheckout([FromBody] MemberCheckoutRequest request)
        {
            try
            {
                var userId = _userContext.GetCurrentAuthenticatedUserId();

                // Nhận kết quả Tuple 3 (order, installments, usages) từ Service
                var (order, installments, usages) = await _serviceProvider.OrderService.HandleMemberCheckoutAsync(
                    userId,
                    request.SelectedCartItemIds,
                    request.PromotionCodes,        // Danh sách mã KM khách nhập
                    request.ChosenFreeProductIds, // Danh sách quà tặng khách chọn
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

                // Truyền đủ 3 tham số vào ResponseMapper
                var response = ResponseMapper.MapToOrderResponseFromOrder(order, installments, usages);

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
        /// Lấy chi tiết đơn hàng: đầy đủ thuộc tính Order
        /// </summary>
        [HttpGet("getOrderDetail/{orderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderDetail([FromRoute] Guid orderId, CancellationToken ct)
        {
            var (order, installments, payments) = await _serviceProvider.OrderService
                .HandleGetOrderDetailAsync(orderId);

            var response = ResponseMapper.MapToOrderDetailResponseFromOrder(order, installments, payments);
            return Ok(ApiResponse<OrderDetailResponse>.OkResponse(response));
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng theo luồng nghiệp vụ.
        /// </summary>
        [HttpPut("{orderId:guid}/status")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(
            [FromRoute] Guid orderId,
            [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _serviceProvider.OrderService
                    .HandleUpdateOrderStatusAsync(orderId, request.Status, request.DeliveredById, request.CourierService, request.CourierTrackingCode);

            var response = ResponseMapper.MapToOrderResponseFromOrder(order);
            return Ok(ApiResponse<OrderResponse>.OkResponse(response));
        }

        /// <summary>
        /// Hủy đơn hàng. Chỉ có thể hủy trước trạng thái Processing.
        /// </summary>
        [HttpPut("{orderId:guid}/cancel")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder([FromRoute] Guid orderId)
        {
            var order = await _serviceProvider.OrderService
                .HandleCancelOrderAsync(orderId);

            var response = ResponseMapper.MapToOrderResponseFromOrder(order);
            return Ok(ApiResponse<OrderResponse>.OkResponse(response));
        }
    }
}
