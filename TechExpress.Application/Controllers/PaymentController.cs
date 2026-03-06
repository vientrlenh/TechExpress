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

        [HttpPost("payments/orders/{orderId:guid}/online/init")]
        [Authorize]
        public async Task<IActionResult> InitOnlinePaymentForOrder(
            [FromRoute] Guid orderId,
            [FromBody] InitOrderOnlinePaymentRequest request,
            CancellationToken ct)
        {
            var init = await _serviceProvider.PaymentService
                .HandleInitOrderOnlinePaymentAsync(orderId, request.Method, request.ReturnUrl, request.CancelUrl, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);
            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        [HttpPost("payments/installments/{installmentId:guid}/online/init")]
        [Authorize]
        public async Task<IActionResult> InitOnlinePaymentForInstallment(
            [FromRoute] Guid installmentId,
            [FromBody] InitInstallmentOnlinePaymentRequest request,
            CancellationToken ct)
        {
            var init = await _serviceProvider.PaymentService
                .HandleInitInstallmentOnlinePaymentAsync(installmentId, request.Method, request.ReturnUrl, request.CancelUrl, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);
            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        [HttpPost("payments/orders/{orderId:guid}/full-settlement/init")]
        [Authorize]
        public async Task<IActionResult> InitFullSettlement(
            [FromRoute] Guid orderId,
            [FromBody] InitOrderOnlinePaymentRequest request,
            CancellationToken ct)
        {
            var init = await _serviceProvider.PaymentService
                .HandleInitFullSettlementAsync(orderId, request.Method, request.ReturnUrl, request.CancelUrl, ct);

            var response = ResponseMapper.MapToInitOnlinePaymentResponse(init);
            return Ok(ApiResponse<InitOnlinePaymentResponse>.OkResponse(response));
        }

        [HttpPost("payments/gateways/payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOsWebhookCallback(
            [FromBody] Webhook webhook,
            CancellationToken ct)
        {
            _logger.LogInformation("PayOS Webhook RAW: {Payload}", JsonSerializer.Serialize(webhook));

            var result = await _serviceProvider.PaymentService.HandlePayOsWebhookAsync(webhook, ct);
            var response = new GatewayCallbackResponse { Ok = result.Ok };
            return Ok(ApiResponse<GatewayCallbackResponse>.OkResponse(response));
        }

        [HttpPost("payments/orders/{orderId:guid}/cash")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CashPayOrder(
            [FromRoute] Guid orderId,
            [FromBody] CashPaymentRequest request,
            CancellationToken ct)
        {
            var payment = await _serviceProvider.PaymentService
                .HandleCashPayOrderAsync(orderId, request.Amount, request.Note, ct);

            var response = ResponseMapper.MapToCashPaymentResponseFromPayment(payment);
            return Ok(ApiResponse<CashPaymentResponse>.OkResponse(response));
        }

        [HttpPost("payments/installments/{installmentId:guid}/cash")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CashPayInstallment(
            [FromRoute] Guid installmentId,
            [FromBody] CashPaymentRequest request,
            CancellationToken ct)
        {
            var payment = await _serviceProvider.PaymentService
                .HandleCashPayInstallmentAsync(installmentId, request.Amount, request.Note, ct);

            var response = ResponseMapper.MapToCashPaymentResponseFromPayment(payment);
            return Ok(ApiResponse<CashPaymentResponse>.OkResponse(response));
        }

        [HttpPost("orders/{orderId:guid}/cancel-refund")]
        [Authorize]
        public async Task<IActionResult> CancelOrderAndRefund(
            [FromRoute] Guid orderId,
            [FromBody] CancelOrderRefundRequest request,
            CancellationToken ct)
        {
            if (request.OrderId != orderId)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "OrderId in route and body must match."
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

        [HttpGet("installments/{installmentId:guid}/payments")]
        [Authorize]
        public async Task<IActionResult> GetInstallmentPayments([FromRoute] Guid installmentId, CancellationToken ct)
        {
            var payments = await _serviceProvider.PaymentService
                .HandleGetPaymentsByInstallmentAsync(installmentId, ct);

            var response = ResponseMapper.MapToPaymentResponseListFromPaymentList(payments);
            return Ok(ApiResponse<List<PaymentResponse>>.OkResponse(response));
        }

        [HttpGet("payments/payos/return")]
        [AllowAnonymous]
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

        [HttpGet("payments/payos/cancel")]
        [AllowAnonymous]
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
