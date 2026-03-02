using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Models;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services;

public class BrandService
{
    private readonly UnitOfWork _unitOfWork;

    public BrandService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Brand> HandleCreateAsync(string name, string? imageUrl)
    {
        if (await _unitOfWork.BrandRepository.ExistsByNameAsync(name.Trim()))
        {
            throw new BadRequestException("Tên thương hiệu đã tồn tại.");
        }

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
            UpdatedAt = DateTimeOffset.Now
        };

        await _unitOfWork.BrandRepository.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return brand;
    }

    public async Task<Pagination<Brand>> HandleGetPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchName = null,
        DateTimeOffset? createdFrom = null,
        DateTimeOffset? createdTo = null,
        Guid? categoryId = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var (items, totalCount) = await _unitOfWork.BrandRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            searchName,
            createdFrom,
            createdTo,
            categoryId);

        return new Pagination<Brand>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Brand> HandleGetByIdAsync(Guid id)
    {
        var brand = await _unitOfWork.BrandRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Thương hiệu không tồn tại.");

        return brand;
    }

    public async Task<Brand> HandleUpdateAsync(Guid id, string? name, string? imageUrl)
    {
        var brand = await _unitOfWork.BrandRepository.FindByIdWithTrackingAsync(id)
            ?? throw new NotFoundException("Thương hiệu không tồn tại.");

        if (!string.IsNullOrWhiteSpace(name))
        {
            var trimmed = name.Trim();
            if (await _unitOfWork.BrandRepository.ExistsByNameExcludingIdAsync(trimmed, id))
            {
                throw new BadRequestException("Tên thương hiệu đã tồn tại.");
            }

            brand.Name = trimmed;
        }

        if (imageUrl != null)
        {
            brand.ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        }

        brand.UpdatedAt = DateTimeOffset.Now;
        await _unitOfWork.SaveChangesAsync();

        return brand;
    }
}

