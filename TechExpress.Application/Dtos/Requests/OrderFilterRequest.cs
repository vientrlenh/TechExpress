using TechExpress.Repository.Enums;
using TechExpress.Service.Enums;

namespace TechExpress.Application.Dtos.Requests
{
    public class OrderFilterRequest
    {
        public string? Search { get; set; }
        public OrderStatus? Status { get; set; }
        public OrderSortBy SortBy { get; set; } = OrderSortBy.OrderDate;
        public SortDirection SortDirection { get; set; } = SortDirection.Desc;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
