using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ProductPCService(UnitOfWork unitOfWork, ProductService productService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
        }


        public async Task<Product> HandleCreateProductPCAsync(
            string name,
            string sku,
            Guid categoryId,
            Guid brandId,
            decimal price,
            int stock,
            int warrantyMonth,
            string description,
            List<string> imageUrls,
            List<CreateProductSpecValueCommand> specValueCommands,
            List<(Guid ComponentProductId, int Quantity)> components)
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

            foreach (var (componentProductId, quantity) in components)
            {
                if (!componentProductDict.TryGetValue(componentProductId, out var componentProduct))
                    throw new NotFoundException($"Không tìm thấy sản phẩm linh kiện có mã {componentProductId}.");

                var requiredQty = quantity * stock;
                if (componentProduct.Stock < requiredQty)
                    throw new BadRequestException(
                        $"Linh kiện '{componentProduct.Name}' (SKU: {componentProduct.Sku}) không đủ tồn kho. Cần: {requiredQty}, Hiện có: {componentProduct.Stock}.");
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

            foreach (var (componentProductId, quantity) in components)
            {
                var componentProduct = componentProductDict[componentProductId];
                componentProduct.Stock -= quantity * stock;
                componentProduct.UpdatedAt = DateTimeOffset.Now;
            }

            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.ProductRepository.FindByIdIncludeAllForPCDetailAsync(pcProduct.Id)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm PC sau khi tạo.");

            return result;
        }
    }
}
