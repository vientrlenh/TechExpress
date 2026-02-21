using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Service;

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

        public PaymentController(ServiceProviders serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

        /// <summary>
        /// Gateway callback (PayOS/VnPay): verify chữ ký + tạo Payment record.
        /// </summary>
        /// <remarks>
        /// <para>Endpoint public, bắt buộc verify chữ ký. Nên idempotent bằng Redis SETNX/lock hoặc unique txn id.</para>
        /// </remarks>
        [HttpPost("payments/gateways/{provider}/callback")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GatewayCallbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GatewayCallback(
            [FromRoute] string provider,
            [FromBody] GatewayCallbackRequest request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Provider is required."
                });
            }

            if (request == null)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Request body is required."
                });
            }

            var result = await _serviceProvider.PaymentService
                .HandleGatewayCallbackAsync(provider, request, ct);

            var response = ResponseMapper.MapToGatewayCallbackResponse(result);

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

    /// <summary>
    /// Request: set payment intent (trả thẳng) cho order.
    /// </summary>
    public class SetPaymentIntentRequest
    {
        /// <summary>
        /// Phương thức thanh toán dự kiến tại checkout.
        /// </summary>
        [Required]
        public PaymentMethod Method { get; set; }
    }

    /// <summary>
    /// Request: set installment intent và tạo schedule theo số tháng.
    /// </summary>
    public class SetInstallmentIntentRequest
    {
        /// <summary>
        /// Số tháng trả góp (1..60).
        /// </summary>
        [Range(1, 60)]
        public int Months { get; set; }
    }

    /// <summary>
    /// Request: init thanh toán online cho order (full).
    /// </summary>
    public class InitOrderOnlinePaymentRequest
    {
        /// <summary>
        /// Cổng thanh toán online: PayOs hoặc VnPay.
        /// </summary>
        [Required]
        public PaymentMethod Method { get; set; }

        /// <summary>
        /// URL client muốn nhận kết quả (optional). Nếu null thì backend dùng default return url.
        /// </summary>
        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// Request: init thanh toán online cho một kỳ installment.
    /// </summary>
    public class InitInstallmentOnlinePaymentRequest
    {
        /// <summary>
        /// Cổng thanh toán online: PayOs hoặc VnPay.
        /// </summary>
        [Required]
        public PaymentMethod Method { get; set; }

    }

    /// <summary>
    /// DTO demo cho callback/return.
    /// Thực tế PayOS/VnPay có payload khác nhau, PaymentService sẽ parse/verify theo provider.
    /// </summary>
    public class GatewayCallbackRequest
    {
        /// <summary>
        /// SessionId được trả từ endpoint init online (Redis session key).
        /// </summary>
        [Required]
        public Guid SessionId { get; set; }

        /// <summary>
        /// Gateway báo thành công hay thất bại.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Số tiền thực tế gateway báo đã thanh toán.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Chữ ký/checksum (tùy gateway). Backend phải verify.
        /// </summary>
        [Required]
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Raw payload (optional) để debug/log hoặc trường hợp gateway trả nhiều field.
        /// </summary>
        public string? Raw { get; set; }
    }

    /// <summary>
    /// Request: staff ghi nhận thu tiền mặt/COD.
    /// </summary>
    public class CashPaymentRequest
    {
        /// <summary>
        /// Số tiền thu.
        /// </summary>
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Ghi chú thu tiền (optional).
        /// </summary>
        public string? Note { get; set; }
    }

    /// <summary>
    /// Request: refund payment.
    /// </summary>
    public class RefundPaymentRequest
    {
        /// <summary>
        /// Lý do refund (optional).
        /// </summary>
        public string? Reason { get; set; }
    }


    /// <summary>
    /// Response: kết quả set intent trả thẳng.
    /// </summary>
    public class SetPaymentIntentResponse
    {
        /// <summary>Order id.</summary>
        public Guid OrderId { get; set; }

        /// <summary>Loại trả: Full.</summary>
        public PaidType PaidType { get; set; }

        /// <summary>Phương thức thanh toán user đã chọn.</summary>
        public PaymentMethod Method { get; set; }
    }

    /// <summary>
    /// Một kỳ trong lịch trả góp.
    /// </summary>
    public class InstallmentItemResponse
    {
        public Guid Id { get; set; }
        public int Period { get; set; }
        public decimal Amount { get; set; }
        public InstallmentStatus Status { get; set; }
        public DateTimeOffset DueDate { get; set; }
    }

    /// <summary>
    /// Response: kết quả tạo schedule trả góp.
    /// </summary>
    public class SetInstallmentIntentResponse
    {
        public Guid OrderId { get; set; }
        public PaidType PaidType { get; set; }
        public int Months { get; set; }
        public List<InstallmentItemResponse> Schedule { get; set; } = new();
    }

    /// <summary>
    /// Response: init online payment.
    /// </summary>
    public class InitOnlinePaymentResponse
    {
        /// <summary>SessionId (Redis) để callback tra cứu.</summary>
        public Guid SessionId { get; set; }

        /// <summary>URL chuyển hướng sang cổng thanh toán (PayOS/VnPay).</summary>
        public string RedirectUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response: callback đã được xử lý hay chưa.
    /// </summary>
    public class GatewayCallbackResponse
    {
        public bool Ok { get; set; }
    }

    /// <summary>
    /// Response: staff ghi nhận thu tiền mặt.
    /// </summary>
    public class CashPaymentResponse
    {
        public long PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
    }

    /// <summary>
    /// Payment item (query).
    /// </summary>
    public class PaymentResponse
    {
        public long Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? InstallmentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
    }

    /// <summary>
    /// Installment item (query).
    /// </summary>
    public class InstallmentResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int Period { get; set; }
        public decimal Amount { get; set; }
        public InstallmentStatus Status { get; set; }
        public DateTimeOffset DueDate { get; set; }
    }

    /// <summary>
    /// Response: refund.
    /// </summary>
    public class RefundPaymentResponse
    {
        public bool Ok { get; set; }
        public long PaymentId { get; set; }
        public string? Reason { get; set; }
    }
}
