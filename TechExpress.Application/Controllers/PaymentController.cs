using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Text.Json;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Application.DTOs.Responses;
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



        /// <summary>
        /// Online init (Order Full): tạo session (Redis) + trả redirectUrl.
        /// </summary>
        /// <remarks>
        /// <para><b>Quan trọng</b>: không tạo Payment record ở bước init.</para>
        /// </remarks>
        [HttpPost("payments/orders/{orderId:guid}/online/init")]
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
                .HandleInitOrderOnlinePaymentAsync(orderId, request.Method, request.ReturnUrl, request.CancelUrl, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);

            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        /// <summary>
        /// Online init (Installment period): tạo session (Redis) + trả redirectUrl.
        /// </summary>
        [HttpPost("payments/installments/{installmentId:guid}/online/init")]
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
                .HandleInitInstallmentOnlinePaymentAsync(installmentId, request.Method, request.ReturnUrl, request.CancelUrl, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);

            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        /// <summary>
        /// Tất toán: Thanh toán toàn bộ số tiền còn lại của đơn hàng trả góp trước thời hạn.
        /// </summary>
        [HttpPost("payments/orders/{orderId:guid}/full-settlement/init")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<InitOnlinePaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitFullSettlement(
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
                .HandleInitFullSettlementAsync(orderId, request.Method, request.ReturnUrl, request.CancelUrl, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);

            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        // =========================
        // 3) GATEWAY CALLBACK
        // =========================
        [HttpPost("payments/gateways/payos/webhook")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GatewayCallbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayOsWebhookCallback(
            [FromBody] Webhook webhook,
            CancellationToken ct)
        {
            if (webhook == null)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Request body is required."
                });
            }

            _logger.LogInformation("PayOS Webhook RAW: {Payload}",
                JsonSerializer.Serialize(webhook));

            var result = await _serviceProvider.PaymentService.HandlePayOsWebhookAsync(webhook, ct);

            var response = new GatewayCallbackResponse { Ok = result.Ok };
            return Ok(ApiResponse<GatewayCallbackResponse>.OkResponse(response));
        }

        // =========================
        // 4) CASH/COD (STAFF)
        // =========================

        /// <summary>
        /// Staff/Admin: ghi nhận thu tiền mặt cho Order (COD/tại quầy).
        /// </summary>
        [HttpPost("payments/orders/{orderId:guid}/cash")]
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
        [HttpPost("payments/installments/{installmentId:guid}/cash")]
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
        // 5) CANCEL ORDER & REFUND
        // =========================

        /// <summary>
        /// Hủy đơn hàng và hoàn tiền 90% số tiền đã thanh toán.
        /// Chỉ có thể hủy trước khi đơn hàng ở trạng thái Processing.
        /// </summary>
        [HttpPost("orders/{orderId:guid}/cancel-refund")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CancelOrderRefundResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrderAndRefund(
            [FromRoute] Guid orderId,
            [FromBody] CancelOrderRefundRequest request,
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

            if (request.OrderId != orderId)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "OrderId in route and body must match."
                });
            }

            if (string.IsNullOrWhiteSpace(request.ToBin))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "ToBin is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.ToAccountNumber))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "ToAccountNumber is required."
                });
            }

            var result = await _serviceProvider.PaymentService
                .HandleCancelOrderAndRefundAsync(
                    orderId,
                    request.ToBin,
                    request.ToAccountNumber,
                    request.Reason,
                    ct);

            var response = ResponseMapper.MapToCancelOrderRefundResponse(result);

            return Ok(ApiResponse<CancelOrderRefundResponse>.OkResponse(response));
        }

        // =========================
        // 6) QUERY
        // =========================

        

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
        //[HttpPost("payments/{paymentId:long}/refund")]
        //[Authorize(Roles = "Admin,Staff")]
        //[ProducesResponseType(typeof(ApiResponse<RefundPaymentResponse>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> RefundPayment(
        //    [FromRoute] long paymentId,
        //    [FromBody] RefundPaymentRequest request,
        //    CancellationToken ct)
        //{
        //    var payment = await _serviceProvider.PaymentService
        //        .HandleRefundPaymentAsync(paymentId, request?.Reason, ct);

        //    // service trả Payment => map ra RefundPaymentResponse
        //    var response = ResponseMapper.MapToRefundPaymentResponse(payment.Id, request?.Reason);

        //    return Ok(ApiResponse<RefundPaymentResponse>.OkResponse(response));
        //}

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
