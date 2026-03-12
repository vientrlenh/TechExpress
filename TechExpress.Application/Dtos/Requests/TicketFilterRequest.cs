using TechExpress.Repository.Enums;
using TechExpress.Service.Enums;

namespace TechExpress.Application.Dtos.Requests;

public class TicketFilterRequest
{
    public TicketStatus? Status { get; set; }
    public TicketSortBy SortBy { get; set; } = TicketSortBy.CreatedAt;
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
