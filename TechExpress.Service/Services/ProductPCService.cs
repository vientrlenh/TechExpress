using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private readonly ComputerCompatibilityService _computerCompatibilityService;

        public ProductPCService(UnitOfWork unitOfWork, ProductService productService, ComputerCompatibilityService computerCompatibilityService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _computerCompatibilityService = computerCompatibilityService;
        }


        public async Task<(Product, List<ComputerComponent>, List<string>?)> HandleCreateProductPCAsync(
            string name,
            string sku,
            Guid categoryId,
            Guid brandId,
            decimal price,
            int warrantyMonth,
            string description,
            List<string> imageUrls,
            List<CreateProductSpecValueCommand> specValueCommands,
            List<AddComputerComponentCommand> componentCommands)
        {
            const int stock = 1;

            var strategy = _unitOfWork.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    if (await _unitOfWork.ProductRepository.ExistsBySkuAsync(sku))
                        throw new BadRequestException($"Mã định danh {sku} đã được sử dụng.");

                    if (await _unitOfWork.ProductRepository.ExistsByNameAsync(name))
                        throw new BadRequestException("Tên sản phẩm đã được sử dụng.");

                    var componentProductIds = componentCommands.Select(c => c.ComponentId).Distinct().ToList();
                    var componentProducts = await _computerCompatibilityService.GetComponentProductsFromRequestedIds(componentProductIds);

                    var compatibilityWarning = await _computerCompatibilityService.CheckComputerCompatibility(componentCommands, componentProducts);

                    foreach (var componentCommand in componentCommands)
                    {

                        var affected = await _unitOfWork.ProductRepository
                            .DecrementStockAtomicAsync(componentCommand.ComponentId, componentCommand.Quantity);

                        if (affected == 0)
                        {
                            throw new BadRequestException(
                                $"Linh kiện '{componentCommand.ComponentId}' không đủ tồn kho cho {componentCommand.Quantity} sản phẩm.");
                        }
                    }

                    var pcProduct = await _productService.PrepareAndAddProductAsync(
                        name, sku, categoryId, brandId, price, stock, warrantyMonth,
                        description, imageUrls, specValueCommands);

                    var computerComponents = componentCommands.Select(c => new ComputerComponent
                    {
                        Id = Guid.NewGuid(),
                        ComputerProductId = pcProduct.Id,
                        ComponentProductId = c.ComponentId,
                        Quantity = c.Quantity
                    }).ToList();

                    await _unitOfWork.ComputerComponentRepository.AddRangeAsync(computerComponents);

                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var result = await _unitOfWork.ProductRepository.FindByIdIncludeCategoryImagesSpecValuesAsync(pcProduct.Id)
                        ?? throw new NotFoundException("Không tìm thấy sản phẩm PC sau khi tạo.");

                    var pcComponents = await _unitOfWork.ComputerComponentRepository
                        .FindByComputerProductIdWithComponentProductAsync(pcProduct.Id);

                    return (result, pcComponents, compatibilityWarning);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<string>> HandleCheckPCCompatibility(List<AddComputerComponentCommand> commands)
        {
            var componentIds = commands.Select(c => c.ComponentId).ToList();
            var components = await _computerCompatibilityService.GetComponentProductsFromRequestedIds(componentIds);
            var results = await _computerCompatibilityService.CheckComputerCompatibility(commands, components);
            return results;
        }
    }
}
