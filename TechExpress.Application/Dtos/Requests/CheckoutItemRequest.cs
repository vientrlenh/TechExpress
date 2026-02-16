using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.DTOs.Requests
{
    public class CheckoutItemRequest
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải ít nhất là 1")]
        public int Quantity { get; set; }
    }
}
