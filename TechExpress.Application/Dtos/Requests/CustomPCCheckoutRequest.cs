using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Requests
{
    public class CustomPCCheckoutRequest
    {
        public List<string>? PromotionCodes { get; set; }
        public List<Guid>? ChosenFreeProductIds { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng.")]
        public required DeliveryType DeliveryType { get; set; }

        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        [MaxLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự.")]
        public string? ReceiverEmail { get; set; }

        [MaxLength(256, ErrorMessage = "Họ tên người nhận không được vượt quá 256 ký tự.")]
        public string? ReceiverFullName { get; set; }

        [MaxLength(512, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 512 ký tự.")]
        public string? ShippingAddress { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string? TrackingPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public required PaidType PaidType { get; set; }

        [MaxLength(20, ErrorMessage = "Số định danh (CCCD) không được vượt quá 20 ký tự.")]
        public string? ReceiverIdentityCard { get; set; }

        [Range(6, 12, ErrorMessage = "Kỳ hạn trả góp chỉ hỗ trợ từ 6 đến 12 tháng.")]
        public int? InstallmentDurationMonth { get; set; }

        [MaxLength(1000, ErrorMessage = "Ghi chú không được dài quá 1000 ký tự.")]
        public string? Notes { get; set; }
    }
}
