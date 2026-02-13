using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Service;
using TechExpress.Service.Enums;
using TechExpress.Service.Utils;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public ProductController(ServiceProviders serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetProductList([FromQuery] ProductFilterRequest request)
        {
            if (request.Page < 1)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Page must be greater than 0"
                });
            }

            var productPagination = await _serviceProvider.ProductService
                .HandleGetProductListWithPaginationAsync(
                    request.Page,
                    request.PageSize,
                    request.SortBy,
                    request.SortDirection,
                    request.Search,
                    request.CategoryId,
                    request.Status
                );

            var response = ResponseMapper
                .MapToProductListResponsePaginationFromProductPagination(productPagination);

            return Ok(ApiResponse<Pagination<ProductListResponse>>.OkResponse(response));
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetProductDetail(Guid id)
        {
            var product = await _serviceProvider.ProductService
                .HandleGetProductDetailAsync(id);

            var response = ResponseMapper
                .MapToProductDetailResponseFromProduct(product);

            return Ok(ApiResponse<ProductDetailResponse>.OkResponse(response));
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            var specValueCmds = RequestMapper.MapToCreateProductSpecValueCommandsFromRequests(request.SpecValues);

            var product = await _serviceProvider.ProductService.HandleCreateProduct(
                request.Name.Trim(),
                request.Sku.Trim(),
                request.CategoryId,
                request.BrandId,
                request.Price,
                request.Stock,
                request.WarrantyMonth,
                request.Description.Trim(),
                request.Images,
                specValueCmds
            );

            var response = ResponseMapper.MapToProductDetailResponseFromProduct(product);

            return CreatedAtAction(nameof(CreateProduct), ApiResponse<ProductDetailResponse>.CreatedResponse(response));
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request)
        {
            var specValueCmds = RequestMapper.MapToCreateProductSpecValueCommandsFromRequests(request.SpecValues);
            var updated = await _serviceProvider.ProductService.HandleUpdateProduct(
                id,
                request.Name,
                request.Sku,
                request.CategoryId,
                request.BrandId,
                request.Price,
                request.Stock,
                request.WarrantyMonth,
                request.Status,
                request.Description,
                specValueCmds
            );

            var response = ResponseMapper.MapToProductDetailResponseFromProduct(updated);
            return Ok(ApiResponse<ProductDetailResponse>.OkResponse(response));
        }



        [HttpPut("images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductImages(
            [FromBody] UpdateProductImagesRequest request)
        {
            var updated = await _serviceProvider.ProductService.HandleReplaceProductImagesAsync(
                request.ProductId,
                request.Images
            );

            var response = ResponseMapper.MapToProductDetailResponseFromProduct(updated);
            return Ok(ApiResponse<ProductDetailResponse>.OkResponse(response));
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _serviceProvider.ProductService.HandleDeleteProductAsync(id);
            return Ok(ApiResponse<string>.OkResponse("Xóa sản phẩm thành công."));
        }


        [HttpGet("ui-latest")]
        public async Task<IActionResult> GetUiNewProducts([FromQuery] int number)
        {
            if (number <= 0 || number > 30)
            {
                return BadRequest("Số lượng sản phẩm mới ra mắt không được vượt quá 30 và dưới 1.");
            }
            var products = await _serviceProvider.ProductService.HandleGetUiNewProductsAsync(number);
            var response = ResponseMapper.MapToProductListResponsesFromProducts(products);
            return Ok(ApiResponse<List<ProductListResponse>>.OkResponse(response));
        }

        [HttpGet("ui")]
        public async Task<IActionResult> GetUiListProducts([FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 12,
             [FromQuery] ProductSortBy sortBy = ProductSortBy.UpdatedAt,
            [FromQuery] SortDirection sortDirection = SortDirection.Asc)
        {
            if (pageSize <= 0 || pageSize > 20) pageSize = 12;
            if (page <= 0) page = 1;
            var pagedProducts =
                await _serviceProvider.ProductService.HandleGetUiProductList(search, categoryId, page, pageSize, sortBy,
                    sortDirection);
            var response = ResponseMapper.MapToProductListResponsePaginationFromProductPagination(pagedProducts);
            return Ok(ApiResponse<Pagination<ProductListResponse>>.OkResponse(response));
        }

    }
}


