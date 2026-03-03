using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class ReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Review> GetBaseQuery(bool tracking = false)
            => (tracking ? _context.Reviews.AsTracking() : _context.Reviews.AsNoTracking())
               .Where(r => !r.IsDeleted);

        public async Task<(List<Review> Items, int TotalCount)> GetPagedByProductIdAsync(
            Guid productId,
            int page,
            int pageSize,
            int? rating,
            bool? hasMedia,
            bool sortByRating,
            bool sortAsc,
            CancellationToken ct = default)
        {
            var query = GetBaseQuery().Where(r => r.ProductId == productId);

            if (rating.HasValue)
                query = query.Where(r => r.Rating == rating.Value);

            if (hasMedia == true)
                query = query.Where(r => r.Medias.Any());

            var totalCount = await query.CountAsync(ct);

            query = sortByRating
                ? (sortAsc ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating))
                : (sortAsc ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt));

            var items = await query
                .Include(r => r.Medias)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Review?> FindByIdAsync(Guid reviewId)
            => await GetBaseQuery()
                .Include(r => r.Medias)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

        public async Task<Review?> FindByIdWithTrackingAsync(Guid reviewId)
            => await GetBaseQuery(tracking: true)
                .Include(r => r.Medias)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }
    }
}
