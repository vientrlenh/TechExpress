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
            var query = _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && !r.IsDeleted);

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
        {
            return await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Medias)
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);
        }

        public async Task<Review?> FindByIdWithTrackingAsync(Guid reviewId)
        {
            return await _context.Reviews
                .AsTracking()
                .Include(r => r.Medias)
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }
    }
}
