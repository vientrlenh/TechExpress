using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests
{
    public class UpdateOrderStatusRequest
    {
        public required OrderStatus Status { get; set; }
    }
}
