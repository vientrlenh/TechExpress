using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;
using TechExpress.Service.Utils;

namespace TechExpress.Application.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandController : ControllerBase
{
    private readonly ServiceProviders _serviceProvider;

    public BrandController(ServiceProviders serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Tạo thương hiệu mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request)
    {
        var brand = await _serviceProvider.BrandService.HandleCreateAsync(
            request.Name,
            request.ImageUrl);

        var response = ResponseMapper.MapToBrandResponseFromBrand(brand);
        return CreatedAtAction(nameof(GetById), new { id = brand.Id }, ApiResponse<BrandResponse>.CreatedResponse(response));
    }

    /// <summary>
    /// Lấy danh sách thương hiệu có phân trang + search/filter
    /// </summary>
    /// <param name="pageNumber">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Kích thước trang (mặc định: 20, tối đa: 100)</param>
    /// <param name="searchName">Tìm kiếm theo tên (contains)</param>
    /// <param name="createdFrom">Lọc từ thời điểm tạo (CreatedAt &gt;= createdFrom)</param>
    /// <param name="createdTo">Lọc đến thời điểm tạo (CreatedAt &lt;= createdTo)</param>
    /// <param name="categoryId">Lọc theo ID danh mục</param>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchName = null,
        [FromQuery] DateTimeOffset? createdFrom = null,
        [FromQuery] DateTimeOffset? createdTo = null,
        [FromQuery] Guid? categoryId = null)
    {
        var pagination = await _serviceProvider.BrandService.HandleGetPagedAsync(
            pageNumber,
            pageSize,
            searchName,
            createdFrom,
            createdTo,
            categoryId);

        var response = ResponseMapper.MapToBrandResponsePaginationFromBrandPagination(pagination);
        return Ok(ApiResponse<Pagination<BrandResponse>>.OkResponse(response));
    }

    /// <summary>
    /// Lấy chi tiết thương hiệu
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var brand = await _serviceProvider.BrandService.HandleGetByIdAsync(id);
        var response = ResponseMapper.MapToBrandResponseFromBrand(brand);
        return Ok(ApiResponse<BrandResponse>.OkResponse(response));
    }

    /// <summary>
    /// Cập nhật thương hiệu
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandRequest request)
    {
        var brand = await _serviceProvider.BrandService.HandleUpdateAsync(
            id,
            request.Name,
            request.ImageUrl);

        var response = ResponseMapper.MapToBrandResponseFromBrand(brand);
        return Ok(ApiResponse<BrandResponse>.OkResponse(response));
    }


}

