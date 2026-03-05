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
    public class PromotionController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public PromotionController(ServiceProviders serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            var startDate = DateMapper.ConvertToDateTimeOffsetFromString(request.StartDate);
            var endDate = DateMapper.ConvertToDateTimeOffsetFromString(request.EndDate);
            var requiredProductCommands = RequestMapper.MapToCreatePromotionRequiredProductCommandListFromRequest(request.RequiredProducts);
            var freeProductCommands = RequestMapper.MapToCreatePromotionFreeProductCommandListFromRequest(request.FreeProducts);

            var promotion = await _serviceProvider.PromotionService.HandleCreatePromotion(
                request.Name.Trim(),
                request.Code,
                request.Description.Trim(),
                request.Type,
                request.Scope,
                request.DiscountValue,
                request.MaxDiscountValue,
                request.MinOrderValue,
                requiredProductCommands,
                request.RequiredProductLogic,
                freeProductCommands,
                request.FreeItemPickCount,
                request.CategoryId,
                request.BrandId,
                request.AppliedProducts,
                request.MinAppliedQuantity,
                request.MaxUsageCount,
                request.MaxUsagePerUser,
                startDate,
                endDate,
                request.IsStackable);

            var response = ResponseMapper.MapToPromotionResponseFromPromotion(promotion);
            return CreatedAtAction(nameof(CreatePromotion), ApiResponse<PromotionResponse>.CreatedResponse(response));
        }
    }
}
