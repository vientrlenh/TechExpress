using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class CreatePromotionFreeProductRequest
{
    [Required(ErrorMessage = "Sản phẩm miễn phí là bắt buộc")]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm miễn phí phải lớn hơn 0")]
    public int Quantity { get; set; }
}
