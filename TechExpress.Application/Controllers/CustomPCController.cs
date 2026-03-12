using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;
using TechExpress.Service.Contexts;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomPCController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;
        private readonly UserContext _userContext;

        public CustomPCController(ServiceProviders serviceProvider, UserContext userContext)
        {
            _serviceProvider = serviceProvider;
            _userContext = userContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomPCBuild(
            [FromHeader(Name = "X-CustomPC-Guest-Session")] string? sessionId,
            [FromBody] CreateCustomPCBuildRequest request)
        {
            var (userId, resolvedSessionId) = ResolveIdentity(sessionId);
            if (userId == null && resolvedSessionId == null)
                return BadRequest("Yêu cầu đăng nhập hoặc cung cấp X-CustomPC-Guest-Session header");

            var newCustomPC = await _serviceProvider.CustomPCService.HandleCreateCustomPCBuild(userId, resolvedSessionId, request.Name.Trim());
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(newCustomPC);
            return CreatedAtAction(nameof(CreateCustomPCBuild), ApiResponse<CustomPCResponse>.CreatedResponse(response));
        }

        [HttpPost("{customPCId}/items")]
        public async Task<IActionResult> AddItemToCustomPC(
            [FromHeader(Name = "X-CustomPC-Guest-Session")] string? sessionId,
            [FromRoute] Guid customPCId,
            [FromBody] AddItemToCustomPCRequest request)
        {
            var (userId, resolvedSessionId) = ResolveIdentity(sessionId);
            if (userId == null && resolvedSessionId == null)
                return BadRequest("Yêu cầu đăng nhập hoặc cung cấp X-CustomPC-Guest-Session header");

            var customPC = await _serviceProvider.CustomPCService.HandleAddItemToCustomPC(userId, resolvedSessionId, customPCId, request.ProductId, request.Quantity);
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(customPC);
            return Ok(ApiResponse<CustomPCResponse>.OkResponse(response));
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomPCs(
            [FromHeader(Name = "X-CustomPC-Guest-Session")] string? sessionId)
        {
            var (userId, resolvedSessionId) = ResolveIdentity(sessionId);
            if (userId == null && resolvedSessionId == null)
                return Ok(ApiResponse<List<CustomPCResponseList>>.OkResponse([]));

            var customPCs = await _serviceProvider.CustomPCService.HandleGetCustomPCs(userId, resolvedSessionId);
            var response = ResponseMapper.MapToCustomPCResponseListFromCustomPCs(customPCs);
            return Ok(ApiResponse<List<CustomPCResponseList>>.OkResponse(response));
        }

        [HttpGet("{customPCId}")]
        public async Task<IActionResult> GetCustomPCById(
            [FromHeader(Name = "X-CustomPC-Guest-Session")] string? sessionId,
            [FromRoute] Guid customPCId)
        {
            var (userId, resolvedSessionId) = ResolveIdentity(sessionId);
            var customPC = await _serviceProvider.CustomPCService.HandleGetCustomPCById(customPCId, userId, resolvedSessionId);
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(customPC);
            return Ok(ApiResponse<CustomPCResponse>.OkResponse(response));
        }

        [HttpDelete("{customPCId}")]
        public async Task<IActionResult> DeleteCustomPC(
            [FromHeader(Name = "X-CustomPC-Guest-Session")] string? sessionId,
            [FromRoute] Guid customPCId)
        {
            var (userId, resolvedSessionId) = ResolveIdentity(sessionId);
            if (userId == null && resolvedSessionId == null)
                return BadRequest("Yêu cầu đăng nhập hoặc cung cấp X-CustomPC-Guest-Session header");

            var response = await _serviceProvider.CustomPCService.HandleDeleteCustomPC(userId, resolvedSessionId, customPCId);
            return Ok(ApiResponse<string>.OkResponse(response));
        }

        private (Guid? userId, string? sessionId) ResolveIdentity(string? guestSessionId)
        {
            var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
            if (userIdStr != null)
                return (Guid.Parse(userIdStr), null);

            return (null, string.IsNullOrWhiteSpace(guestSessionId) ? null : guestSessionId);
        }
    }
}
