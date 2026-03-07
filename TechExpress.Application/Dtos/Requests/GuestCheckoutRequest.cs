using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.DTOs.Requests
{
    public class GuestCheckoutRequest
    {
        [Required(ErrorMessage = "Danh sách sản phẩm không được để trống")]
        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một sản phẩm")]
        public required List<CheckoutItemRequest> Items { get; set; }

        // --- NEW FIELDS FOR PROMOTION ---
        public List<string>? PromotionCodes { get; set; } // Danh sách mã KM
        public List<Guid>? ChosenFreeProductIds { get; set; } // Quà khách chọn
        // --------------------------------

        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng")]
        public required DeliveryType DeliveryType { get; set; }

        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [MaxLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
        public string? ReceiverEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận")]
        [MaxLength(256, ErrorMessage = "Tên người nhận không được vượt quá 256 ký tự")]
        public required string ReceiverFullName { get; set; }

        [MaxLength(512, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 512 ký tự")]
        public string? ShippingAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public required string TrackingPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public required PaidType PaidType { get; set; }

        [MaxLength(20, ErrorMessage = "Số định danh (CCCD) không hợp lệ")]
        public string? ReceiverIdentityCard { get; set; }

        [Range(6, 12, ErrorMessage = "Kỳ hạn trả góp chỉ từ 6 đến 12 tháng")]
        public int? InstallmentDurationMonth { get; set; }

        [MaxLength(512, ErrorMessage = "Ghi chú không được vượt quá 512 ký tự")]
        public string? Notes { get; set; }
    }
}