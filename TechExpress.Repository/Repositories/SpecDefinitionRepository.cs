using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class SpecDefinitionRepository
{
    private readonly ApplicationDbContext _context;
    public SpecDefinitionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SpecDefinition?> FindByIdIncludeCategoryAsync(Guid id)
    {
        return await _context.SpecDefinitions
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<SpecDefinition?> FindByIdAsync(Guid id)
    {
        return await _context.SpecDefinitions.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SpecDefinition?> FindByIdIncludeCategoryWithTrackingAsync(Guid id)
    {
        return await _context.SpecDefinitions
            .AsTracking()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SpecDefinition?> FindByIdWithTrackingAsync(Guid id)
    {
        return await _context.SpecDefinitions.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
    }
    
    public async Task<(List<SpecDefinition> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchName,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo)
    {
        var query = _context.SpecDefinitions
            .Where(s => !s.IsDeleted)
            .Include(s => s.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var keyword = searchName.Trim();
            query = query.Where(s => EF.Functions.Like(s.Name, $"%{keyword}%"));
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= createdTo.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<SpecDefinition>> GetAllAsync()
    {
        return await _context.SpecDefinitions
            .Where(s => !s.IsDeleted)
            .Include(s => s.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(SpecDefinition specDefinition)
    {
        await _context.SpecDefinitions.AddAsync(specDefinition);
    }

    public void Update(SpecDefinition specDefinition)
    {
        _context.SpecDefinitions.Update(specDefinition);
    }

    public void Remove(SpecDefinition specDefinition)
    {
        _context.SpecDefinitions.Remove(specDefinition);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.SpecDefinitions
            .AnyAsync(s => s.Name == name && !s.IsDeleted);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId)
    {
        return await _context.SpecDefinitions
            .AnyAsync(s => s.Name == name && s.Id != excludeId && !s.IsDeleted);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _context.SpecDefinitions
            .AnyAsync(s => s.Code == code && !s.IsDeleted);
    }

    public async Task<bool> ExistsByCodeExcludingIdAsync(string code, Guid excludeId)
    {
        return await _context.SpecDefinitions
            .AnyAsync(s => s.Code == code && s.Id != excludeId && !s.IsDeleted);
    }

    public async Task<bool> HasRelatedProductSpecValuesAsync(Guid specDefinitionId)
    {
        return await _context.ProductSpecValues
            .AnyAsync(psv => psv.SpecDefinitionId == specDefinitionId);
    }

    public async Task<bool> CategoryExistsAsync(Guid categoryId)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == categoryId && !c.IsDeleted);
    }

    
    public async Task<List<SpecDefinition>> FindByCategoryIdAsync(Guid categoryId)
    {
        return await _context.SpecDefinitions.Where(s => s.CategoryId == categoryId).OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public async Task<(List<SpecDefinition>, int)> FindByCategoryIdWithPagingAsync(Guid categoryId, int pageNumber)
    {
        int pageSize = 20;
        var query = _context.SpecDefinitions.Where(s => s.CategoryId == categoryId);
        var totalCount = await query.CountAsync();
        var specs = await query.OrderByDescending(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return (specs, totalCount);
    }

    public async Task<List<SpecDefinition>> FindSpecDefinitionListByCategoryIdAndIsNotDeletedAsync(Guid categoryId)
    {
        return await _context.SpecDefinitions
            .Where(s => s.CategoryId == categoryId && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<HashSet<SpecDefinition>> FindSpecDefinitionSetByCategoryIdAndIsNotDeletedAsync(Guid categoryId)
    {
        return await _context.SpecDefinitions.Where(s => s.CategoryId == categoryId && !s.IsDeleted).ToHashSetAsync();
    }

    public async Task<Dictionary<Guid, Dictionary<Guid, SpecDefinition>>> FindDictByCategoryIdsAndIsNotDeletedAsync(List<Guid> categoryIds)
    {
        var specs = await _context.SpecDefinitions.Where(s => categoryIds.Contains(s.CategoryId) && !s.IsDeleted).ToListAsync();

        return specs.GroupBy(s => s.CategoryId).ToDictionary(g => g.Key, g => g.ToDictionary(s => s.Id, s => s));
    }
}
