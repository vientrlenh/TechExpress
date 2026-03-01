using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.DTOs.Requests;
using TechExpress.Application.DTOs.Responses;
using TechExpress.Service;
using TechExpress.Service.Utils;


namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public CategoryController(ServiceProviders serviceProviders)
        {
            _serviceProvider = serviceProviders;
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            // Lấy từng field từ request để truyền vào Service
            var category = await _serviceProvider.CategoryService.HandleCreateCategory(
                request.Name,
                request.Description,
                request.ParentCategoryId,
                request.ImageUrl
            );

            var response = ResponseMapper.MapToCategoryResponseFromCategory(category);
            return Ok(ApiResponse<CategoryResponse>.OkResponse(response));
        }

        [HttpPatch("update{id}")]
        [Authorize(Roles = "Admin")] //  Chỉ Admin mới có quyền cập nhật
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _serviceProvider.CategoryService.HandleUpdateCategory(
                id,
                request.Name,
                request.Description,
                request.ParentCategoryId,
                request.ImageUrl,
                request.Status
            );

            var response = ResponseMapper.MapToCategoryResponseFromCategory(category);
            return Ok(ApiResponse<CategoryResponse>.OkResponse(response));
        }


        //=======================================Category List Controller =======================================//
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetList([FromQuery] CategoryFilterRequest filter)
        {
            var pagination = await _serviceProvider.CategoryService.HandleGetCategories(
                filter.SearchName,
                filter.ParentId,
                filter.Status,
                filter.Page
            );

            var responseItems = pagination.Items
                .Select(ResponseMapper.MapToCategoryResponseFromCategory)
                .ToList();

            var response = new Pagination<CategoryResponse>
            {
                Items = responseItems,
                TotalCount = pagination.TotalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(ApiResponse<Pagination<CategoryResponse>>.OkResponse(response));
        }
        //======================== =======Category Controller Delete Handling===============================
        // Path: TechExpress.Application/Controllers/CategoryController.cs
        /// <summary>
        /// Xóa category theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Gọi logic xử lý từ Service
            var resultMessage = await _serviceProvider.CategoryService.HandleDeleteCategory(id);

            // Trả về thông báo xác nhận thành công
            return Ok(ApiResponse<string>.OkResponse(resultMessage));
        }
        
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FindCategoryDetails([FromRoute] Guid id)
        {
            var category = await _serviceProvider.CategoryService.HandleFindCategoryDetailsByIdAsync(id);
            var response = ResponseMapper.MapToCategoryResponseFromCategory(category);
            return Ok(ApiResponse<CategoryResponse>.OkResponse(response));
        }


        [HttpGet("ui")]
        public async Task<IActionResult> GetUiCategoryList()
        {
            var categories = await _serviceProvider.CategoryService.HandleGetUICategoryListAsync();
            var response = ResponseMapper.MapToCategoryResponseListFromCategories(categories);
            return Ok(ApiResponse<List<CategoryResponse>>.OkResponse(response));
        }


        [HttpGet("parent")]
        public async Task<IActionResult> GetParentCategories()
        {
            var categories = await _serviceProvider.CategoryService.HandleGetParentCategoriesAsync();
            var response = ResponseMapper.MapToCategoryResponseListFromCategories(categories);
            return Ok(ApiResponse<List<CategoryResponse>>.OkResponse(response));
        }
    }
}
