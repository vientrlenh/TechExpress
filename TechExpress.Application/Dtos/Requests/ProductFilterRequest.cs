using TechExpress.Repository.Enums;
using TechExpress.Service.Enums;

namespace TechExpress.Application.Dtos.Requests
{
    public class ProductFilterRequest
    {
        public string? Search { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public ProductStatus? Status { get; set; }
        public ProductSortBy SortBy { get; set; } = ProductSortBy.UpdatedAt;

        public SortDirection SortDirection { get; set; } = SortDirection.Asc;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
