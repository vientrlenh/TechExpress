using System;
using Microsoft.Identity.Client;
using PayOS.Exceptions;
using TechExpress.Repository;
using TechExpress.Repository.Models;
using TechExpress.Repository.Repositories;

namespace TechExpress.Service.Services;

public class CustomPCService
{

    private readonly UnitOfWork _unitOfWork;

    public CustomPCService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomPC> HandleCreateCustomPCBuild(Guid userId, string name)
    {
        int count = await _unitOfWork.CustomPCRepository.CountByUserIdAsync(userId);
        if (count >= 20)
        {
            throw new BadRequestException($"Người dùng chỉ có thể sở hữu tối đa 20 cấu hình tự chọn cùng lúc");
        }
        CustomPC customPC = new CustomPC
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
        };
        await _unitOfWork.CustomPCRepository.AddAsync(customPC);
        await _unitOfWork.SaveChangesAsync();

        CustomPC newCustomPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPC.Id) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn: {customPC.Id}");
        return newCustomPC;
    }

    public async Task<CustomPC> HandleAddItemToCustomPC(Guid userId, Guid customPCId, Guid productId, int quantity)
    {
        CustomPC customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsWithTrackingAsync(customPCId) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn {customPCId}");
        if (customPC.UserId != userId)
        {
            throw new ForbiddenException($"Bạn không có quyền thực hiện trên cấu hình tự chọn này");
        }
        if (!await _unitOfWork.ProductRepository.ExistsByIdAndAvailableAsync(productId))
        {
            throw new NotFoundException($"Không tìm thấy sản phẩm {productId}");
        }
        if (customPC.Items.Any(i => i.ProductId == productId))
        {
            if (quantity == 0)
            {
                return customPC;
            }
            CustomPCItem item = customPC.Items.First(i => i.ProductId == productId);
            item.Quantity += quantity;
            if (item.Quantity <= 0)
            {
                customPC.Items.Remove(item);
            }
        }
        else
        {
            if (quantity <= 0)
            {
                return customPC;
            }
            CustomPCItem item = new CustomPCItem
            {
                CustomPCId = customPCId,
                ProductId = productId,
                Quantity = quantity,
            };
            customPC.Items.Add(item);
        }
        customPC.UpdatedAt = DateTimeOffset.Now;
        await _unitOfWork.SaveChangesAsync();
        return customPC;
    }

    public async Task<List<CustomPC>> HandleGetCustomPCs(Guid userId)
    {
        return await _unitOfWork.CustomPCRepository.FindByUserIdIncludeItemsWithSplitQueryAsync(userId);
    }

    public async Task<string> HandleDeleteCustomPC(Guid userId, Guid customPCId)
    {
        var customPC = await _unitOfWork.CustomPCRepository.FindByIdWithTrackingAsync(customPCId) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn: {customPCId}");
        if (customPC.UserId != userId)
        {
            throw new ForbiddenException($"Bạn không có quyền thực hiện hành động trên cấu hình này");
        }
        var removalName = customPC.Name;
        _unitOfWork.CustomPCRepository.Remove(customPC);
        await _unitOfWork.SaveChangesAsync();
        return $"Cấu hình {removalName} đã xóa thành công";
    }
}
