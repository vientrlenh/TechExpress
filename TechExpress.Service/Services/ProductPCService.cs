using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;

namespace TechExpress.Service.Services
{
    public class ProductPCService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ProductService _productService;
        private readonly PCComponentCompatibilityService _compatibilityService;

        public ProductPCService(UnitOfWork unitOfWork, ProductService productService, PCComponentCompatibilityService compatibilityService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _compatibilityService = compatibilityService;
        }


        public async Task<(Product Product, List<ComputerComponent> Components)> HandleCreateProductPCAsync(
            string name,
            string sku,
            Guid categoryId,
            Guid brandId,
            decimal price,
            int warrantyMonth,
            string description,
            List<string> imageUrls,
            List<CreateProductSpecValueCommand> specValueCommands,
            List<(Guid ComponentProductId, int Quantity)> components)
        {
            // Tự động set stock = 1 cho chức năng build PC
            const int stock = 1;

            // Bắt đầu transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (await _unitOfWork.ProductRepository.ExistsBySkuAsync(sku))
                    throw new BadRequestException($"Mã định danh {sku} đã được sử dụng.");

                if (await _unitOfWork.ProductRepository.ExistsByNameAsync(name))
                    throw new BadRequestException("Tên sản phẩm đã được sử dụng.");

                if (components.Count == 0)
                    throw new BadRequestException("PC phải có ít nhất 1 linh kiện.");

                var componentProductIds = components.Select(c => c.ComponentProductId).Distinct().ToList();
                var componentProducts = await _unitOfWork.ProductRepository.FindByIdsWithTrackingAsync(componentProductIds);

                var componentProductDict = componentProducts.ToDictionary(p => p.Id);
                var missingIds = componentProductIds.Where(id => !componentProductDict.ContainsKey(id)).ToList();
                if (missingIds.Count > 0)
                    throw new NotFoundException(
                        "Không tìm thấy sản phẩm linh kiện với mã: " + string.Join(", ", missingIds) + ".");

                var compatibilityResult = await _compatibilityService.ValidatePcComponentsAsync(components);
                if (!compatibilityResult.IsCompatible)
                    throw new BadRequestException(
                        "Các linh kiện không tương thích: " + string.Join(" ", compatibilityResult.Errors));

                foreach (var (componentProductId, quantity) in components)
                {
                    var requiredQty = quantity * stock;

                    var affected = await _unitOfWork.ProductRepository
                        .DecrementStockAtomicAsync(componentProductId, requiredQty);

                    if (affected == 0)
                    {
                        var componentProduct = componentProductDict[componentProductId];
                        throw new BadRequestException(
                            $"Linh kiện '{componentProduct.Name}' (SKU: {componentProduct.Sku}) không đủ tồn kho cho {requiredQty} sản phẩm.");
                    }
                }

                var pcProduct = await _productService.PrepareAndAddProductAsync(
                    name, sku, categoryId, brandId, price, stock, warrantyMonth,
                    description, imageUrls, specValueCommands);

                var computerComponents = components.Select(c => new ComputerComponent
                {
                    Id = Guid.NewGuid(),
                    ComputerProductId = pcProduct.Id,
                    ComponentProductId = c.ComponentProductId,
                    Quantity = c.Quantity
                }).ToList();

                await _unitOfWork.ComputerComponentRepository.AddRangeAsync(computerComponents);

                await _unitOfWork.SaveChangesAsync();

                // Commit transaction nếu thành công
                await transaction.CommitAsync();

                var result = await _unitOfWork.ProductRepository.FindByIdIncludeCategoryImagesSpecValuesAsync(pcProduct.Id)
                    ?? throw new NotFoundException("Không tìm thấy sản phẩm PC sau khi tạo.");

                var pcComponents = await _unitOfWork.ComputerComponentRepository
                    .FindByComputerProductIdWithComponentProductAsync(pcProduct.Id);

                return (result, pcComponents);
            }
            catch
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
