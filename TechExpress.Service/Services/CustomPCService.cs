using PayOS.Exceptions;
using TechExpress.Repository;
using TechExpress.Repository.Models;

namespace TechExpress.Service.Services;

public class CustomPCService
{

    private readonly UnitOfWork _unitOfWork;

    public CustomPCService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomPC> HandleCreateCustomPCBuild(Guid? userId, string? sessionId, string name)
    {
        if (!userId.HasValue && string.IsNullOrWhiteSpace(sessionId))
        {
            throw new BadRequestException("Không nhận diện được người dùng hoặc session hiện tại");
        }
        int count = userId.HasValue
            ? await _unitOfWork.CustomPCRepository.CountByUserIdAsync(userId.Value)
            : await _unitOfWork.CustomPCRepository.CountBySessionIdAsync(sessionId!);

        if (count >= 20)
        {
            throw new BadRequestException($"Người dùng chỉ có thể sở hữu tối đa 20 cấu hình tự chọn cùng lúc");
        }
        CustomPC customPC = new CustomPC
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = userId.HasValue ? null : sessionId,
            Name = name,
        };
        await _unitOfWork.CustomPCRepository.AddAsync(customPC);
        await _unitOfWork.SaveChangesAsync();

        CustomPC newCustomPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPC.Id) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn: {customPC.Id}");
        return newCustomPC;
    }

    public async Task<CustomPC> HandleAddItemToCustomPC(Guid? userId, string? sessionId, Guid customPCId, Guid productId, int quantity)
    {
        CustomPC customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsWithTrackingAsync(customPCId) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn {customPCId}");
        if (!IsOwner(customPC, userId, sessionId))
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
                return await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsThenIncludeProductWithSplitQueryAsync(customPC.Id) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn {customPC.Id}");
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
                return await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsThenIncludeProductWithSplitQueryAsync(customPC.Id) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn {customPC.Id}");
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

        var newCustomPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsThenIncludeProductWithSplitQueryAsync(customPC.Id) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn {customPC.Id}");
        return newCustomPC;
    }

    public async Task<List<CustomPC>> HandleGetCustomPCs(Guid? userId, string? sessionId)
    {
        if (!userId.HasValue && string.IsNullOrWhiteSpace(sessionId))
        {
            throw new BadRequestException("Không tìm thấy người dùng hoặc session hiện tại");
        }
        if (userId.HasValue)
            return await _unitOfWork.CustomPCRepository.FindByUserIdIncludeItemsThenIncludeProductWithSplitQueryAsync(userId.Value);

        return await _unitOfWork.CustomPCRepository.FindBySessionIdIncludeItemsThenIncludeProductWithSplitQueryAsync(sessionId!);
    }

    public async Task<CustomPC> HandleGetCustomPCById(Guid id, Guid? userId, string? sessionId)
    {
        var customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsThenIncludeProductWithSplitQueryAsync(id)
            ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn: {id}");

        if (!await CanAccess(customPC, userId, sessionId))
            throw new ForbiddenException("Bạn không có quyền xem cấu hình tự chọn này");

        return customPC;
    }

    public async Task<string> HandleDeleteCustomPC(Guid? userId, string? sessionId, Guid customPCId)
    {
        var customPC = await _unitOfWork.CustomPCRepository.FindByIdWithTrackingAsync(customPCId) ?? throw new NotFoundException($"Không tìm thấy cấu hình tự chọn: {customPCId}");
        if (!IsOwner(customPC, userId, sessionId))
        {
            throw new ForbiddenException($"Bạn không có quyền thực hiện hành động trên cấu hình này");
        }
        var removalName = customPC.Name;
        _unitOfWork.CustomPCRepository.Remove(customPC);
        await _unitOfWork.SaveChangesAsync();
        return $"Cấu hình {removalName} đã xóa thành công";
    }

    private static bool IsOwner(CustomPC pc, Guid? userId, string? sessionId)
    {
        if (userId.HasValue) {
            return userId == pc.UserId;
        }
        return sessionId != null && pc.SessionId == sessionId;
    }

    private async Task<bool> CanAccess(CustomPC pc, Guid? userId, string? sessionId)
    {
        if (IsOwner(pc, userId, sessionId)) return true;
        if (userId.HasValue)
        {
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ?? throw new NotFoundException($"Không tìm thấy người dùng {userId.Value}");
            return !user.IsCustomerUser() && pc.IsStaffAccessible;
        }
        return false;
    }
}
