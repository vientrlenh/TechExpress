using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;
using TechExpress.Service.Services;
using System;

namespace TechExpress.Application.Controllers
{
    /// <summary>
    /// Controller để kiểm tra bảo hành sản phẩm khi hỗ trợ khách hàng.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class WarrantySupportController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public WarrantySupportController(ServiceProviders serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Kiểm tra bảo hành theo TicketId (dùng DateTimeOffset.Now).
        /// </summary>
        [HttpPost("tickets/{ticketId:guid}/check-warranty")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckWarrantyByTicketId(
            [FromRoute] Guid ticketId,
            CancellationToken ct)
        {
            var result = await _serviceProvider.WarrantySupportService
                .CheckWarrantyByTicketIdAsync(ticketId, checkDate: null, ct);

            var response = ResponseMapper.MapToWarrantyCheckResponseFromResult(result);
            return Ok(ApiResponse<WarrantyCheckResponse>.OkResponse(response));
        }

        /// <summary>
        /// Kiểm tra bảo hành theo TicketId với ngày kiểm tra tùy chỉnh.
        /// </summary>
        [HttpPost("tickets/{ticketId:guid}/check-warranty-custom-date")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckWarrantyByTicketIdWithCustomDate(
            [FromRoute] Guid ticketId,
            [FromBody] CheckWarrantyByTicketIdRequest request,
            CancellationToken ct)
        {
         

            var result = await _serviceProvider.WarrantySupportService
                .CheckWarrantyByTicketIdAsync(ticketId, request.CheckDate, ct);

            var response = ResponseMapper.MapToWarrantyCheckResponseFromResult(result);
            return Ok(ApiResponse<WarrantyCheckResponse>.OkResponse(response));
        }

        /// <summary>
        /// Kiểm tra bảo hành theo OrderItemId (dùng DateTimeOffset.Now).
        /// </summary>
        [HttpPost("order-items/{orderItemId:long}/check-warranty")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckWarrantyByOrderItemId(
            [FromRoute] long orderItemId,
            CancellationToken ct)
        {
            var result = await _serviceProvider.WarrantySupportService
                .CheckWarrantyByOrderItemIdAsync(orderItemId, ticketId: null, checkDate: null, ct);

            var response = ResponseMapper.MapToWarrantyCheckResponseFromResult(result);
            return Ok(ApiResponse<WarrantyCheckResponse>.OkResponse(response));
        }

        /// <summary>
        /// Kiểm tra bảo hành theo OrderItemId với ngày kiểm tra tùy chỉnh và optional ticketId.
        /// </summary>
        [HttpPost("order-items/{orderItemId:long}/check-warranty-custom-date")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckWarrantyByOrderItemIdWithCustomDate(
            [FromRoute] long orderItemId,
            [FromBody] CheckWarrantyByOrderItemIdRequest request,
            CancellationToken ct)
        {
           
            var result = await _serviceProvider.WarrantySupportService
                .CheckWarrantyByOrderItemIdAsync(orderItemId, request.TicketId, request.CheckDate, ct);

            var response = ResponseMapper.MapToWarrantyCheckResponseFromResult(result);
            return Ok(ApiResponse<WarrantyCheckResponse>.OkResponse(response));
        }
    }
}
