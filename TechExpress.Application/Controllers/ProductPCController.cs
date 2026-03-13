using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductPCController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public ProductPCController(ServiceProviders serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Tạo sản phẩm PC từ các linh kiện.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductPC([FromBody] CreateProductPCRequest request)
        {
            var specValueCmds = RequestMapper.MapToCreateProductSpecValueCommandsFromRequests(request.SpecValues);

            var componentCommands = RequestMapper.MapToAddComputerComponentCommandListFromRequest(request.Components);

            var (product, pcComponents, compatibilityWarning) = await _serviceProvider.ProductPCService.HandleCreateProductPCAsync(
                request.Name.Trim(),
                request.Sku.Trim(),
                request.CategoryId,
                request.BrandId,
                request.Price,
                request.WarrantyMonth,
                request.Description.Trim(),
                request.Images,
                specValueCmds,
                componentCommands
            );

            var response = ResponseMapper.MapToPCDetailsWithCompatibilityWarningResponse(product, pcComponents, compatibilityWarning);

            return CreatedAtAction(nameof(CreateProductPC), ApiResponse<PCDetailsWithCompatibilityWarningResponse>.CreatedResponse(response));
        }

        [HttpPost("compatibility")]
        public async Task<IActionResult> CheckPCCompatibility([FromBody] CheckPCCompatibilityRequest request)
        {
            var commands = RequestMapper.MapToAddComputerComponentCommandListFromAddItemToCustomPCRequests(request.Schema);
            var response = await _serviceProvider.ProductPCService.HandleCheckPCCompatibility(commands);
            return Ok(ApiResponse<List<string>>.OkResponse(response));
        }
    }
}
