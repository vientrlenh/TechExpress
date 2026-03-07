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
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateCustomPCBuild([FromBody] CreateCustomPCBuildRequest request)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var newCustomPC = await _serviceProvider.CustomPCService.HandleCreateCustomPCBuild(userId, request.Name.Trim());
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(newCustomPC);
            return CreatedAtAction(nameof(CreateCustomPCBuild), ApiResponse<CustomPCResponse>.CreatedResponse(response));
        }

        [HttpPost("items/{customPCId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddItemToCustomPC([FromRoute] Guid customPCId, [FromBody] AddItemToCustomPCRequest request)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var customPC = await _serviceProvider.CustomPCService.HandleAddItemToCustomPC(userId, customPCId, request.ProductId, request.Quantity);
            var response = ResponseMapper.MapToCustomPCResponseFromCustomPC(customPC);
            return Ok(ApiResponse<CustomPCResponse>.OkResponse(response));
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCustomPCs()
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var customPCs = await _serviceProvider.CustomPCService.HandleGetCustomPCs(userId);
            var response = ResponseMapper.MapToCustomPCResponseListFromCustomPCs(customPCs);
            return Ok(ApiResponse<List<CustomPCResponse>>.OkResponse(response));
        }


        [HttpDelete("{customPCId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteCustomPC([FromRoute] Guid customPCId)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var response = await _serviceProvider.CustomPCService.HandleDeleteCustomPC(userId, customPCId);
            return Ok(ApiResponse<string>.OkResponse(response));
        }
    }
}
