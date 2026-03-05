using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;


        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Product> BuildFilteredQuery(
            string? search,
            List<Guid>? categoryIds,
            ProductStatus? status,
            Guid? brandId = null)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            if (categoryIds != null && categoryIds.Count > 0)
                query = query.Where(p => categoryIds.Contains(p.CategoryId));

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    p.Sku.ToLower().Contains(s));
            }

            return query;
        }




        private async Task<(List<Product> Products, int TotalCount)> ExecutePagedQueryAsync(
            IQueryable<Product> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<(List<Product> Products, int TotalCount)> FindProductsPagedSortByPriceAsync(
    int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds, ProductStatus? status, Guid? brandId = null)
        {
            var query = BuildFilteredQuery(search, categoryIds, status, brandId);

            query = isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }




        public async Task<(List<Product> Products, int TotalCount)> FindProductsPagedSortByCreatedAtAsync(
    int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds, ProductStatus? status, Guid? brandId = null)
        {
            var query = BuildFilteredQuery(search, categoryIds, status, brandId);

            query = isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }



        public async Task<(List<Product> Products, int TotalCount)> FindProductsPagedSortByStockQtyAsync(
    int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds, ProductStatus? status, Guid? brandId = null)
        {
            var query = BuildFilteredQuery(search, categoryIds, status, brandId);

            query = isDescending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }



        public async Task<(List<Product> Products, int TotalCount)> FindProductsPagedSortByUpdatedAtAsync(
    int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds, ProductStatus? status, Guid? brandId = null)
        {
            var query = BuildFilteredQuery(search, categoryIds, status, brandId);

            query = isDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }



        public async Task<bool> ExistsBySkuAsync(string sku)
        {
            var s = sku.Trim().ToLower();
            return await _context.Products.AnyAsync(p => p.Sku.ToLower() == s);
        }

        public async Task AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task<Product?> FindByIdIncludeCategoryAndImagesAndSpecValuesThenIncludeSpecDefinitionWithSplitQueryAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.SpecValues)
                    .ThenInclude(sv => sv.SpecDefinition)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Lấy Product theo id. Components lấy riêng qua ComputerComponentRepository (right join).
        /// </summary>
        public async Task<Product?> FindByIdIncludeCategoryImagesSpecValuesAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.SpecValues)
                    .ThenInclude(sv => sv.SpecDefinition)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> FindByIdAsync(Guid id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> FindByIdWithTrackingAsync(Guid id)
        {
            return await _context.Products
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> FindByIdWithNoTrackingAsync(Guid id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> FindByIdsWithTrackingAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return [];
            return await _context.Products
                .AsTracking()
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<List<Product>> FindByIdsIncludeCategoryAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return [];
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<bool> ExistsBySkuExcludingProductIdAsync(string sku, Guid excludeProductId)
        {
            var s = sku.Trim().ToLower();

            return await _context.Products.AnyAsync(p =>
                p.Id != excludeProductId &&
                p.Sku == s
            );
        }

        public async Task HardDeleteProductByIdAsync(Guid productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
                _context.Products.Remove(product);
        }


        public async Task<List<Product>> GetProductsByCategoryIdWithTrackingAsync(Guid categoryId)
        {
            // Lấy danh sách sản phẩm thuộc danh mục để chuẩn bị cập nhật trạng thái
            return await _context.Products
                .AsTracking()
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<bool> AnyProductsInCategoryAsync(Guid id)
        {
            // Kiểm tra xem có sản phẩm nào đang thuộc danh mục này không
            return await _context.Products.AnyAsync(p => p.CategoryId == id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Products.AnyAsync(p => p.Name == name);
        }

        public async Task<bool> ExistsByNameExcludingProductIdAsync(string name, Guid excludingId)
        {
            return await _context.Products.AnyAsync(p => p.Name == name && p.Id != excludingId);
        }

        public async Task<List<Product>> FindAvailableNewProductsIncludeCategoryIncludeImagesIncludeSpecValuesThenIncludeSpecDefinitionWithSplitQueryAsync(int number)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.SpecValues)
                    .ThenInclude(sv => sv.SpecDefinition)
                .AsSplitQuery()
                .Where(p => p.Status == ProductStatus.Available)
                .OrderByDescending(p => p.CreatedAt)
                .Take(number)
                .ToListAsync();
        }
        
        public async Task<(List<Product> Products, int TotalCount)> FindUiProductsPagedSortByPriceAsync(
            int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds)
        {
            var query = BuildUiFilteredQuery(search, categoryIds);

            query = isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }




        public async Task<(List<Product> Products, int TotalCount)> FindUiProductsPagedSortByCreatedAtAsync(
            int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds)
        {
            var query = BuildUiFilteredQuery(search, categoryIds);

            query = isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }

        public async Task<(List<Product> Products, int TotalCount)> FindUiProductsPagedSortByUpdatedAtAsync(
            int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds)
        {
            var query = BuildUiFilteredQuery(search, categoryIds);

            query = isDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }

        public async Task<(List<Product> Products, int TotalCount)> FindUiProductsPagedSortByStockQtyAsync(
            int page, int pageSize, bool isDescending, string? search, List<Guid>? categoryIds)
        {
            var query = BuildUiFilteredQuery(search, categoryIds);

            query = isDescending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }

        private IQueryable<Product> BuildUiFilteredQuery(
            string? search,
            List<Guid>? categoryIds)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.Stock > 0 && p.Status == ProductStatus.Available)
                .AsQueryable();

            if (categoryIds != null && categoryIds.Count > 0)
                query = query.Where(p => categoryIds.Contains(p.CategoryId));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    p.Sku.ToLower().Contains(s));
            }

            return query;
        }



        public async Task<List<Product>> FindTopSellingProductsAsync(int count)
        {
            return await _context.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.ProductId) // Nhóm theo ProductId
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity) // Tính tổng số lượng bán ra
                })
                .OrderByDescending(x => x.TotalSold) // Sắp xếp giảm dần
                .Take(count)
                .Join(_context.Products
                    .Include(p => p.Category) // Bao gồm thông tin danh mục
                    .Include(p => p.Images)   // Bao gồm hình ảnh
                    .Where(p => p.Status == ProductStatus.Available), // Chỉ lấy sản phẩm đang kinh doanh
                    top => top.ProductId,
                    p => p.Id,
                    (top, p) => p)
                .ToListAsync();
        }


        // Sử dụng tính năng của auto-transaction của EF Core để thực hiện cập nhật số lượng tồn kho một cách nguyên tử, tránh tình trạng oversell khi có nhiều khách hàng mua cùng lúc.
        public async Task<int> DecrementStockAtomicAsync(Guid productId, int quantity)
        {
            // ExecuteUpdateAsync (EF Core 7+) cập nhật trực tiếp xuống DB 
            // và trả về số hàng bị ảnh hưởng.
            return await _context.Products
                .Where(p => p.Id == productId && p.Stock >= quantity)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Stock, p => p.Stock - quantity)
                    .SetProperty(p => p.UpdatedAt, DateTimeOffset.Now));
        }

        // Tăng số lượng tồn kho một cách nguyên tử khi hủy đơn hàng.
        public async Task<int> IncrementStockAtomicAsync(Guid productId, int quantity)
        {
            return await _context.Products
                .Where(p => p.Id == productId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Stock, p => p.Stock + quantity)
                    .SetProperty(p => p.UpdatedAt, DateTimeOffset.Now));
                    
        


        }

        public async Task<List<Product>> FindByIdsIncludeCategoryAsync(List<Guid> ids)
        {
            return await _context.Products.Include(p => p.Category).Where(p => ids.Contains(p.Id)).ToListAsync();
        }
    }
}
