using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Dtos.Requests;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            return Ok(null);
        }
    }
}
