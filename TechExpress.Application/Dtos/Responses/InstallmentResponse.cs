using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Responses
{
    public class InstallmentResponse
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public int Period { get; set; }
        public decimal Amount { get; set; }
        public InstallmentStatus Status { get; set; }
        public DateTimeOffset DueDate { get; set; }
    }


}
