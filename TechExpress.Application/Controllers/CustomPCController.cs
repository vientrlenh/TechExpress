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
        public async Task<IActionResult> CreateCustomPCBuild([FromBody] CreateCustomPCBuildRequest request)
        {
            var (userId, sessionId) = ResolveIdentity();
            if (userId == null && sessionId == null)
                return BadRequest("Yêu cầu đăng nhập hoặc cung cấp X-CustomPC-Guest-Session header");

            var newCustomPC = await _serviceProvider.CustomPCService.HandleCreateCustomPCBuild(userId, sessionId, request.Name.Trim());
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(newCustomPC);
            return CreatedAtAction(nameof(CreateCustomPCBuild), ApiResponse<CustomPCResponse>.CreatedResponse(response));
        }

        [HttpPost("{customPCId}/items")]
        public async Task<IActionResult> AddItemToCustomPC([FromRoute] Guid customPCId, [FromBody] AddItemToCustomPCRequest request)
        {
            var (userId, sessionId) = ResolveIdentity();
            if (userId == null && sessionId == null)
                return BadRequest("Yêu cầu đăng nhập hoặc cung cấp X-CustomPC-Guest-Session header");

            var customPC = await _serviceProvider.CustomPCService.HandleAddItemToCustomPC(userId, sessionId, customPCId, request.ProductId, request.Quantity);
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(customPC);
            return Ok(ApiResponse<CustomPCResponse>.OkResponse(response));
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomPCs()
        {
            var (userId, sessionId) = ResolveIdentity();
            if (userId == null && sessionId == null)
                return Ok(ApiResponse<List<CustomPCResponseList>>.OkResponse([]));

            var customPCs = await _serviceProvider.CustomPCService.HandleGetCustomPCs(userId, sessionId);
            var response = ResponseMapper.MapToCustomPCResponseListFromCustomPCs(customPCs);
            return Ok(ApiResponse<List<CustomPCResponseList>>.OkResponse(response));
        }

        [HttpGet("{customPCId}")]
        public async Task<IActionResult> GetCustomPCById([FromRoute] Guid customPCId)
        {
            var (userId, sessionId) = ResolveIdentity();
            var customPC = await _serviceProvider.CustomPCService.HandleGetCustomPCById(customPCId, userId, sessionId);
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(customPC);
            return Ok(ApiResponse<CustomPCResponse>.OkResponse(response));
        }

        [HttpDelete("{customPCId}")]
        public async Task<IActionResult> DeleteCustomPC([FromRoute] Guid customPCId)
        {
            var (userId, sessionId) = ResolveIdentity();
            if (userId == null && sessionId == null)
                return BadRequest("Yêu cầu đăng nhập hoặc cung cấp X-CustomPC-Guest-Session header");

            var response = await _serviceProvider.CustomPCService.HandleDeleteCustomPC(userId, sessionId, customPCId);
            return Ok(ApiResponse<string>.OkResponse(response));
        }

        private (Guid? userId, string? sessionId) ResolveIdentity()
        {
            var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
            if (userIdStr != null)
                return (Guid.Parse(userIdStr), null);

            const string headerName = "X-CustomPC-Guest-Session";
            var sessionId = Request.Headers.TryGetValue(headerName, out var val) ? val.ToString() : null;
            return (null, sessionId);
        }
    }
}
