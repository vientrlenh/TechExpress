using TechExpress.Repository.Enums;
using TechExpress.Service.Enums;

namespace TechExpress.Application.Dtos.Requests
{
    public class CustomerOrderFilterRequest
    {
        public OrderStatus? OrderStatus { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public SortDirection SortDirection { get; set; } = SortDirection.Desc;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
