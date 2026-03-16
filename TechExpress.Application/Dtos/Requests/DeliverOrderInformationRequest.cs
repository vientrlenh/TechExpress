using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests
{
    public class DeliverOrderInformationRequest
    {
        [StringLength(100, ErrorMessage = "Dịch vụ vận chuyển không được quá 100 ký tự")]
        public string? CourierService { get; set; }

        [StringLength(100, ErrorMessage = "Mã dịch vụ vận chuyển không được vượt quá 100 ký tự")]
        public string? CourierTrackingCode { get; set; }

        [Required(ErrorMessage = "Self deliver is required")]
        public bool IsSelfDeliver { get; set; }
    }
}
