using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class ProductSpecValueRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductSpecValueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductSpecValue>> FindByProductIdWithTrackingAsync(Guid productId)
        {
            return await _context.ProductSpecValues
                .AsTracking()
                .Where(x => x.ProductId == productId)
                .ToListAsync();
        }

        public async Task<ProductSpecValue?> FindByProductIdAndSpecDefinitionIdAsync(Guid productId, Guid specDefinitionId)
        {
            return await _context.ProductSpecValues
                .AsNoTracking()
                .Include(psv => psv.SpecDefinition)
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.SpecDefinitionId == specDefinitionId);
        }

        public async Task AddAsync(ProductSpecValue entity)
        {
            await _context.ProductSpecValues.AddAsync(entity);
        }

        public async Task RemoveRangeProductSpec(List<ProductSpecValue> specValues)
        {
            _context.ProductSpecValues.RemoveRange(specValues);
        }
    }
}
