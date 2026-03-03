using TechExpress.Service.Enums;

namespace TechExpress.Application.Dtos.Requests
{
    public class ReviewFilterRequest
    {
        public int? Rating { get; set; }
        public bool? HasMedia { get; set; }
        public ReviewSortBy SortBy { get; set; } = ReviewSortBy.CreatedAt;
        public SortDirection SortDirection { get; set; } = SortDirection.Desc;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
