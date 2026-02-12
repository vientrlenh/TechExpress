using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class ComputerComponentRepository
    {
        private readonly ApplicationDbContext _context;

        public ComputerComponentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<ComputerComponent> components)
        {
            await _context.ComputerComponents.AddRangeAsync(components);
        }

        public async Task<List<ComputerComponent>> FindByComputerProductIdWithComponentProductAsync(Guid computerProductId)
        {
            return await _context.ComputerComponents
                .AsNoTracking()
                .Include(cc => cc.ComponentProduct)
                .Where(cc => cc.ComputerProductId == computerProductId)
                .ToListAsync();
        }


        public async Task<List<ComputerComponent>> FindByComputerProductIdWithComponentProductTrackingAsync(Guid computerProductId)
        {
            return await _context.ComputerComponents
                .AsTracking()
                .Include(cc => cc.ComponentProduct)
                .Where(cc => cc.ComputerProductId == computerProductId)
                .ToListAsync();
        }
    }
}
