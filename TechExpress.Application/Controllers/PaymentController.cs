using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Service;
using TechExpress.Service.Services;

namespace TechExpress.Application.Controllers
{
    /// <summary>
    /// API thanh toán &amp; trả góp (Installment schedule) cho Order.
    /// - Online: init tạo session + trả redirectUrl; callback ghi nhận Payment (Success/Failed)
    /// - Cash/COD: staff ghi nhận Payment khi đã thu tiền
    /// - Installment: tạo lịch trả góp theo kỳ (Installment rows)
    /// </summary>
    /// <remarks>
    /// <para><b>Luồng tổng quát</b></para>
    /// <list type="number">
    ///   <item><description><b>Checkout Intent</b>: user chọn <c>Full</c> hoặc <c>Installment</c>.</description></item>
    ///   <item><description><b>Online Init</b>: tạo session Redis TTL và trả <c>redirectUrl</c>. Không tạo Payment record.</description></item>
    ///   <item><description><b>Gateway Callback</b>: verify chữ ký + idempotent, tạo Payment + update Order/Installment.</description></item>
    ///   <item><description><b>Cash/COD</b>: Staff/Admin ghi nhận thu tiền trực tiếp (Payment Success).</description></item>
    /// </list>
    /// </remarks>
    [Route("api")]
    [ApiController]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ServiceProviders serviceProvider, ILogger<PaymentController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // =========================
        // 1) CHECKOUT - INTENT
        // =========================

        /// <summary>
        /// Checkout: chọn trả thẳng (Full) + payment method.
        /// </summary>
        /// <remarks>
        /// <para><b>Request</b></para>
        /// <code>{ "method": "PayOs" }</code>
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
        [HttpPut("orders/{orderId:guid}/payment-intent")]
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
        [HttpPut("orders/{orderId:guid}/installment-intent")]
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

        // =========================
        // 2) ONLINE INIT (REDIRECT)
        // =========================

        /// <summary>
        /// Online init (Order Full): tạo session (Redis) + trả redirectUrl.
        /// </summary>
        /// <remarks>
        /// <para><b>Quan trọng</b>: không tạo Payment record ở bước init.</para>
        /// </remarks>
        [HttpPost("orders/{orderId:guid}/payments/online/init")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<InitOnlinePaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitOnlinePaymentForOrder(
            [FromRoute] Guid orderId,
            [FromBody] InitOrderOnlinePaymentRequest request,
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

            if (request.Method is not (PaymentMethod.PayOs or PaymentMethod.VnPay))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Online payment only supports PayOs or VnPay."
                });
            }

            var init = await _serviceProvider.PaymentService
                .HandleInitOrderOnlinePaymentAsync(orderId, request.Method, request.ReturnUrl, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);

            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        /// <summary>
        /// Online init (Installment period): tạo session (Redis) + trả redirectUrl.
        /// </summary>
        [HttpPost("installments/{installmentId:guid}/payments/online/init")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<InitOnlinePaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitOnlinePaymentForInstallment(
            [FromRoute] Guid installmentId,
            [FromBody] InitInstallmentOnlinePaymentRequest request,
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

            //if (request.Amount <= 0)
            //{
            //    return BadRequest(new ErrorResponse
            //    {
            //        StatusCode = StatusCodes.Status400BadRequest,
            //        Message = "Amount must be greater than 0."
            //    });
            //}

            if (request.Method is not (PaymentMethod.PayOs or PaymentMethod.VnPay))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Online payment only supports PayOs or VnPay."
                });
            }

            var init = await _serviceProvider.PaymentService
                .HandleInitInstallmentOnlinePaymentAsync(installmentId, request.Method, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);

            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        // =========================
        // 3) GATEWAY CALLBACK
        // =========================
        [HttpPost("payments/gateways/payos/callback")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GatewayCallbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayOsWebhookCallback(
            [FromBody] PayOsWebhookRequest request,
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

            _logger.LogInformation("PayOS Webhook RAW: {Payload}",
                JsonSerializer.Serialize(request));

            var result = await _serviceProvider.PaymentService.HandlePayOsWebhookAsync(request, ct);

            var response = new GatewayCallbackResponse { Ok = result.Ok };
            return Ok(ApiResponse<GatewayCallbackResponse>.OkResponse(response));
        }

        // =========================
        // 4) CASH/COD (STAFF)
        // =========================

        /// <summary>
        /// Staff/Admin: ghi nhận thu tiền mặt cho Order (COD/tại quầy).
        /// </summary>
        [HttpPost("orders/{orderId:guid}/payments/cash")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<CashPaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CashPayOrder(
            [FromRoute] Guid orderId,
            [FromBody] CashPaymentRequest request,
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

            if (request.Amount <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Amount must be greater than 0."
                });
            }

            var payment = await _serviceProvider.PaymentService
                .HandleCashPayOrderAsync(orderId, request.Amount, request.Note, ct);

            var response = ResponseMapper.MapToCashPaymentResponseFromPayment(payment);

            return Ok(ApiResponse<CashPaymentResponse>.OkResponse(response));
        }

        /// <summary>
        /// Staff/Admin: ghi nhận thu tiền mặt cho một kỳ Installment.
        /// </summary>
        [HttpPost("installments/{installmentId:guid}/payments/cash")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<CashPaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CashPayInstallment(
            [FromRoute] Guid installmentId,
            [FromBody] CashPaymentRequest request,
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

            if (request.Amount <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Amount must be greater than 0."
                });
            }

            var payment = await _serviceProvider.PaymentService
                .HandleCashPayInstallmentAsync(installmentId, request.Amount, request.Note, ct);

            var response = ResponseMapper.MapToCashPaymentResponseFromPayment(payment);

            return Ok(ApiResponse<CashPaymentResponse>.OkResponse(response));
        }

        // =========================
        // 5) QUERY
        // =========================

        /// <summary>
        /// Query: danh sách Payment theo Order.
        /// </summary>
        [HttpGet("orders/{orderId:guid}/payments")]
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
        [HttpGet("orders/{orderId:guid}/installment")]
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
        /// Query: danh sách Payment theo một kỳ Installment.
        /// </summary>
        [HttpGet("installments/{installmentId:guid}/payments")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PaymentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInstallmentPayments([FromRoute] Guid installmentId, CancellationToken ct)
        {
            var payments = await _serviceProvider.PaymentService
                .HandleGetPaymentsByInstallmentAsync(installmentId, ct);

            var response = ResponseMapper.MapToPaymentResponseListFromPaymentList(payments);

            return Ok(ApiResponse<List<PaymentResponse>>.OkResponse(response));
        }

        // =========================
        // 6) OPTIONAL REFUND
        // =========================

        /// <summary>
        /// Staff/Admin: Refund một Payment (optional).
        /// </summary>
        [HttpPost("payments/{paymentId:long}/refund")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<RefundPaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefundPayment(
            [FromRoute] long paymentId,
            [FromBody] RefundPaymentRequest request,
            CancellationToken ct)
        {
            var payment = await _serviceProvider.PaymentService
                .HandleRefundPaymentAsync(paymentId, request?.Reason, ct);

            // service trả Payment => map ra RefundPaymentResponse
            var response = ResponseMapper.MapToRefundPaymentResponse(payment.Id, request?.Reason);

            return Ok(ApiResponse<RefundPaymentResponse>.OkResponse(response));
        }

        // =========================
        // 3b) TEMP RETURN/CANCEL (NO WEBHOOK YET) - PAYOS
        // =========================

        /// <summary>
        /// PayOS return URL (tạm thời): client bị redirect về đây khi thanh toán thành công.
        /// Vì chưa có webhook URL chính thức, FE gọi endpoint này để backend ghi nhận Payment + update Order/Installment.
        /// </summary>
        /// <remarks>
        /// Query params:
        /// - orderCode: PayOS orderCode (ưu tiên)
        /// - sessionId: nếu bạn tự append sessionId vào returnUrl khi init
        /// </remarks>
        [HttpGet("payments/payos/return")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GatewayCallbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayOsReturn(
            [FromQuery] long? orderCode,
            [FromQuery] Guid? sessionId,
            CancellationToken ct)
        {
            if (!orderCode.HasValue && !sessionId.HasValue)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "orderCode or sessionId is required."
                });
            }

            var result = await _serviceProvider.PaymentService
                .HandlePayOsReturnOrCancelAsync(
                    isSuccess: true,
                    orderCode: orderCode,
                    sessionId: sessionId,
                    ct: ct);

            var response = new GatewayCallbackResponse { Ok = result.Ok };
            return Ok(ApiResponse<GatewayCallbackResponse>.OkResponse(response));
        }

        /// <summary>
        /// PayOS cancel URL (tạm thời): client bị redirect về đây khi huỷ/timeout.
        /// Backend ghi nhận Payment Failed + update Order/Installment.
        /// </summary>
        [HttpGet("payments/payos/cancel")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GatewayCallbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayOsCancel(
            [FromQuery] long? orderCode,
            [FromQuery] Guid? sessionId,
            CancellationToken ct)
        {
            if (!orderCode.HasValue && !sessionId.HasValue)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "orderCode or sessionId is required."
                });
            }

            var result = await _serviceProvider.PaymentService
                .HandlePayOsReturnOrCancelAsync(
                    isSuccess: false,
                    orderCode: orderCode,
                    sessionId: sessionId,
                    ct: ct);

            var response = new GatewayCallbackResponse { Ok = result.Ok };
            return Ok(ApiResponse<GatewayCallbackResponse>.OkResponse(response));
        }

    }
}
