using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class AddItemToCustomPCRequest
{
    [Required(ErrorMessage = "Id của sản phẩm là bắt buộc")]
    public required Guid ProductId { get; set; }

    [Required(ErrorMessage = "Số lượng sản phẩm là bắt buộc")]
    [Range(-1000, 1000, ErrorMessage = "Sản phẩm không được dưới -1000 và trên 1000")]
    public required int Quantity { get; set; }
}
