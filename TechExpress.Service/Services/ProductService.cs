using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;
using TechExpress.Service.Enums;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services
{
    public class ProductService
    {
        private readonly UnitOfWork _unitOfWork;

        public ProductService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> HandleGetProductDetailAsync(Guid productId)
        {
            var product = await _unitOfWork.ProductRepository.FindByIdIncludeCategoryAndImagesAndSpecValuesThenIncludeSpecDefinitionWithSplitQueryAsync(productId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            return product;
        }


        public async Task<Pagination<Product>> HandleGetProductListWithPaginationAsync(
    int page,
    int pageSize,
    ProductSortBy sortBy,
    SortDirection sortDirection,
    string? search,
    Guid? categoryId,
    ProductStatus? status)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var isDescending = sortDirection == SortDirection.Desc;

            List<Guid>? categoryIds = null;
            if (categoryId.HasValue)
            {
                var descendants = await _unitOfWork.CategoryRepository
                    .GetDescendantCategoryIdsAsync(categoryId.Value);

                categoryIds = new List<Guid>(descendants.Count + 1) { categoryId.Value };
                categoryIds.AddRange(descendants);
            }

            var (products, totalCount) = sortBy switch
            {
                ProductSortBy.Price => await _unitOfWork.ProductRepository
                    .FindProductsPagedSortByPriceAsync(page, pageSize, isDescending, search, categoryIds, status),

                ProductSortBy.CreatedAt => await _unitOfWork.ProductRepository
                    .FindProductsPagedSortByCreatedAtAsync(page, pageSize, isDescending, search, categoryIds, status),

                ProductSortBy.StockQty => await _unitOfWork.ProductRepository
                    .FindProductsPagedSortByStockQtyAsync(page, pageSize, isDescending, search, categoryIds, status),

                _ => await _unitOfWork.ProductRepository
                    .FindProductsPagedSortByUpdatedAtAsync(page, pageSize, isDescending, search, categoryIds, status)
            };

            return new Pagination<Product>
            {
                Items = products,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Product> HandleCreateProduct(
              string name,
              string sku,
              Guid categoryId,
              Guid brandId,
              decimal price,
              int stockQty,
              int warrantyMonth,
              string description,
              List<string> imageUrls,
              List<CreateProductSpecValueCommand> specValueCommands)
        {
            var product = await PrepareAndAddProductAsync(name, sku, categoryId, brandId, price, stockQty, warrantyMonth, description, imageUrls, specValueCommands);
            await _unitOfWork.SaveChangesAsync();
            product = await _unitOfWork.ProductRepository.FindByIdIncludeCategoryAndImagesAndSpecValuesThenIncludeSpecDefinitionWithSplitQueryAsync(product.Id) ?? throw new NotFoundException($"Không tìm thấy sản phẩm đã tạo xong");
            return product;
        }


        public async Task<Product> PrepareAndAddProductAsync(
              string name,
              string sku,
              Guid categoryId,
              Guid brandId,
              decimal price,
              int stockQty,
              int warrantyMonth,
              string description,
              List<string> imageUrls,
              List<CreateProductSpecValueCommand> specValueCommands)
        {
            if (await _unitOfWork.ProductRepository.ExistsBySkuAsync(sku))
                throw new BadRequestException("Mã định danh đã được sử dụng.");

            if (await _unitOfWork.ProductRepository.ExistsByNameAsync(name))
                throw new BadRequestException("Tên sản phẩm đã được sử dụng.");

            if (!await _unitOfWork.BrandRepository.ExistsByIdAsync(brandId))
                throw new BadRequestException("Không tìm thấy thương hiệu.");

            var category = await _unitOfWork.CategoryRepository.FindCategoryByIdAsync(categoryId);
            if (category == null || category.IsDeleted)
                throw new NotFoundException("Không tìm thấy danh mục.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Sku = sku,
                CategoryId = categoryId,
                BrandId = brandId,
                Price = price,
                Stock = stockQty,
                WarrantyMonth = warrantyMonth,
                Status = ProductStatus.Available,
                Description = description
            };

            if (imageUrls.Count > 0)
            {
                foreach (var imageUrl in imageUrls)
                {
                    product.Images.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imageUrl.Trim(),
                    });
                }
            }

            if (specValueCommands.Count > 0)
            {
                await BuildNewProductSpecValues(categoryId, specValueCommands, product, false);
            }

            await _unitOfWork.ProductRepository.AddProductAsync(product);
            return product;
        }

        private static ProductSpecValue BuildProductSpecValue(Guid productId, SpecDefinition def, string rawValue)
        {
            var specValue = new ProductSpecValue
            {
                ProductId = productId,
                SpecDefinitionId = def.Id,
                UpdatedAt = DateTimeOffset.Now
            };

            switch (def.AcceptValueType)
            {
                case SpecAcceptValueType.Text:
                    specValue.TextValue = rawValue;
                    break;

                case SpecAcceptValueType.Number:
                    if (!int.TryParse(rawValue, out var n))
                        throw new BadRequestException($"'{def.Name}' phải là số nguyên.");
                    specValue.NumberValue = n;
                    break;

                case SpecAcceptValueType.Decimal:
                    if (!decimal.TryParse(rawValue, out var d))
                        throw new BadRequestException($"'{def.Name}' phải là số thập phân.");
                    specValue.DecimalValue = d;
                    break;

                case SpecAcceptValueType.Bool:
                    if (!bool.TryParse(rawValue, out var b))
                        throw new BadRequestException($"'{def.Name}' phải là true/false.");
                    specValue.BoolValue = b;
                    break;

                default:
                    throw new BadRequestException($"Kiểu dữ liệu '{def.AcceptValueType}' chưa được hỗ trợ.");
            }

            return specValue;
        }


        public async Task<Product> HandleUpdateProduct(
            Guid productId,
            string? name,
            string? sku,
            Guid? categoryId,
            Guid? brandId,
            decimal? price,
            int? stock,
            int? warrantyMonth,
            ProductStatus? status,
            string? description, 
            List<CreateProductSpecValueCommand> specValueCommands)
        {

            var product = await _unitOfWork.ProductRepository.FindByIdWithTrackingAsync(productId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            if (!string.IsNullOrWhiteSpace(sku)) { 
                if (await _unitOfWork.ProductRepository.ExistsBySkuExcludingProductIdAsync(sku, excludeProductId: productId))
                {
                    throw new BadRequestException("Mã định danh đã tồn tại.");
                }
                product.Sku = sku.Trim();
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (await _unitOfWork.ProductRepository.ExistsByNameExcludingProductIdAsync(name, productId))
                {
                    throw new BadRequestException("Tên sản phẩm đã tồn tại.");
                }
                product.Name = name.Trim();
            }

            if (categoryId.HasValue)
            {
                var newCategory = await _unitOfWork.CategoryRepository.FindCategoryByIdAsync(categoryId.Value);
                if (newCategory == null || newCategory.IsDeleted)
                {
                    throw new NotFoundException("Không tìm thấy danh mục.");
                }
                if (categoryId.Value != product.CategoryId)
                {
                    await BuildNewProductSpecValues(categoryId.Value, specValueCommands, product, true);
                }
                else
                {
                    await UpdateProductSpecValueOnExistingCategory(categoryId.Value, specValueCommands, product);
                }
            }
            else 
            {
                await UpdateProductSpecValueOnExistingCategory(product.CategoryId, specValueCommands, product);
            }

            if (brandId.HasValue)
            {
                if (!await _unitOfWork.BrandRepository.ExistsByIdAsync(brandId.Value))
                {
                    throw new NotFoundException($"Không tìm thấy thương hiệu {brandId.Value}");
                }
                product.BrandId = brandId.Value;
            }

            if (price.HasValue)
            {
                product.Price = price.Value;
            }
            if (stock.HasValue)
            {
                product.Stock = stock.Value;
            }
            if (status.HasValue && product.Status != status)
            {
                product.Status = status.Value;
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                product.Description = description;
            }
            if (warrantyMonth.HasValue)
            {
                product.WarrantyMonth = warrantyMonth.Value;
            }

            product.UpdatedAt = DateTimeOffset.Now;
            await _unitOfWork.SaveChangesAsync();

            product = await _unitOfWork.ProductRepository.FindByIdIncludeCategoryAndImagesAndSpecValuesThenIncludeSpecDefinitionWithSplitQueryAsync(product.Id) ?? throw new NotFoundException($"Không tìm thấy sản phẩm đã tạo xong");

            return product;
        }

        public async Task<Product> HandleReplaceProductImagesAsync(
            Guid productId,
            List<string>? imageUrls)
        {
            var product = await _unitOfWork.ProductRepository
                .FindByIdWithTrackingAsync(productId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            await _unitOfWork.ProductImageRepository.DeleteByProductIdAsync(productId);

            if (imageUrls != null && imageUrls.Count > 0)
            {
                var images = new List<ProductImage>();
                foreach (var url in imageUrls)
                {
                    if (string.IsNullOrWhiteSpace(url)) continue;

                    images.Add(new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = url.Trim()
                    });
                }

                if (images.Count > 0)
                {
                    await _unitOfWork.ProductImageRepository.AddRangeAsync(images);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.ProductRepository.FindByIdAsync(productId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm sau khi cập nhật ảnh.");

            return updated;
        }


        public async Task HandleDeleteProductAsync(Guid productId)
        {
            var product = await _unitOfWork.ProductRepository
                .FindByIdWithNoTrackingAsync(productId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            var components = await _unitOfWork.ComputerComponentRepository
                .FindByComputerProductIdWithComponentProductTrackingAsync(productId);

            if (components.Count > 0)
            {
                foreach (var component in components)
                {
                    var qtyToRestore = component.Quantity * product.Stock;
                    component.ComponentProduct.Stock += qtyToRestore;
                    component.ComponentProduct.UpdatedAt = DateTimeOffset.Now;
                }
            }

            try
            {
                await _unitOfWork.ProductRepository.HardDeleteProductByIdAsync(productId);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {

                if (product.Status != ProductStatus.Unavailable)
                {
                    product.Status = ProductStatus.Unavailable;
                    product.UpdatedAt = DateTimeOffset.Now;
                }

                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task BuildNewProductSpecValues(Guid categoryId, List<CreateProductSpecValueCommand> specValueCommands, Product product, bool isEditing)
        {
            if (isEditing)
            {
                var productSpecValues = await _unitOfWork.ProductSpecValueRepository.FindByProductIdWithTrackingAsync(product.Id);
                await _unitOfWork.ProductSpecValueRepository.RemoveRangeProductSpec(productSpecValues);
                product.CategoryId = categoryId;
            }
            var specDefinitionSet = await _unitOfWork.SpecDefinitionRepository.FindSpecDefinitionSetByCategoryIdAndIsNotDeletedAsync(categoryId);
            var specDefinitionDict = specDefinitionSet.ToDictionary(s => s.Id);
            var requiredSpecIds = specDefinitionSet.Where(x => x.IsRequired).Select(x => x.Id).ToHashSet();
            
            if (requiredSpecIds.Count > 0)
            {
                if (specValueCommands.Count == 0)
                {
                    throw new BadRequestException("Danh mục có chứa thông số bắt buộc, giá trị phải được định nghĩa khi khởi tạo sản phẩm.");
                }
                var requestedSpecIds = specValueCommands.Select(s => s.SpecDefinitionId).ToHashSet();
                if (!requiredSpecIds.IsSubsetOf(requestedSpecIds))
                {
                    throw new BadRequestException("Thiếu thông số bắt buộc của sản phẩm.");
                }
            }
            var categorySpecIds = specDefinitionSet.Select(s => s.Id).ToHashSet();
            foreach (var command in specValueCommands)
            {
                if (!specDefinitionDict.TryGetValue(command.SpecDefinitionId, out var def))
                {
                    throw new BadRequestException($"Thông số {command.SpecDefinitionId} không tồn tại trong {categoryId}");
                }
                var psv = BuildProductSpecValue(product.Id, def, command.Value);
                product.SpecValues.Add(psv);
            }
        }

        private static void UpdateProductSpecValue(ProductSpecValue specValue, SpecDefinition def, string rawValue)
        {
            
            switch (def.AcceptValueType)
            {
                case SpecAcceptValueType.Text:
                    specValue.TextValue = rawValue;
                    return;

                case SpecAcceptValueType.Number:
                    if (!int.TryParse(rawValue, out var n))
                        throw new BadRequestException($"'{def.Name}' phải là số nguyên.");
                    specValue.NumberValue = n;
                    return;

                case SpecAcceptValueType.Decimal:
                    if (!decimal.TryParse(rawValue, out var d))
                        throw new BadRequestException($"'{def.Name}' phải là số thập phân.");
                    specValue.DecimalValue = d;
                    return;

                case SpecAcceptValueType.Bool:
                    if (!bool.TryParse(rawValue, out var b))
                        throw new BadRequestException($"'{def.Name}' phải là true/false.");
                    specValue.BoolValue = b;
                    return;

                default:
                    throw new BadRequestException($"Kiểu dữ liệu '{def.AcceptValueType}' chưa được hỗ trợ.");
            }
        }

        private async Task UpdateProductSpecValueOnExistingCategory(Guid categoryId, List<CreateProductSpecValueCommand> specValueCommands, Product product)
        {
            if (!await _unitOfWork.CategoryRepository.ExistByIdAndIsNotDeleted(categoryId))
            {
                throw new NotFoundException($"Không tìm thấy danh mục {categoryId}");
            }
            product.CategoryId = categoryId;

            if (specValueCommands.Count > 0)
            {
                var specDefinitionSet = await _unitOfWork.SpecDefinitionRepository.FindSpecDefinitionSetByCategoryIdAndIsNotDeletedAsync(categoryId);
                var existingSpecValues = await _unitOfWork.ProductSpecValueRepository.FindByProductIdWithTrackingAsync(product.Id);

                var specDefinitionDict = specDefinitionSet.ToDictionary(s => s.Id);
                var existingSpecValueDict = existingSpecValues.ToDictionary(s => s.SpecDefinitionId);

                foreach (var command in specValueCommands)
                {
                    if (!specDefinitionDict.TryGetValue(command.SpecDefinitionId, out var def))
                    {
                        throw new NotFoundException($"Thông số {command.SpecDefinitionId} không tồn tại trong danh mục {categoryId}");
                    }
                    
                    if (existingSpecValueDict.TryGetValue(command.SpecDefinitionId, out var productSpecValue))
                    {
                        UpdateProductSpecValue(productSpecValue, def, command.Value);
                    }
                    else
                    {
                        var psv = BuildProductSpecValue(product.Id, def, command.Value);
                        product.SpecValues.Add(psv);
                    }
                }
            }
        }

        public async Task<List<Product>> HandleGetUiNewProductsAsync(int number)
        {
            return await _unitOfWork.ProductRepository.FindAvailableNewProductsIncludeCategoryIncludeImagesIncludeSpecValuesThenIncludeSpecDefinitionWithSplitQueryAsync(number);
        }
    }

}

