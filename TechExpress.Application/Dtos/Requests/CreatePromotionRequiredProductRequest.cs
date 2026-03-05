using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class CreatePromotionRequiredProductRequest
{
    [Required(ErrorMessage = "Sản phẩm điều kiện là bắt buộc")]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối thiểu phải lớn hơn 0")]
    public int MinQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
    public int? MaxQuantity { get; set; }
}
