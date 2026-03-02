using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;


public class ProductPCComponentRequest
{
    [Required(ErrorMessage = "Mã sản phẩm linh kiện không được để trống")]
    public required Guid ComponentProductId { get; set; }

    [Required(ErrorMessage = "Số lượng linh kiện không được để trống")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng linh kiện phải lớn hơn 0")]
    public required int Quantity { get; set; }
}

public class CreateProductPCRequest
{
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(256, ErrorMessage = "Tên sản phẩm không được vượt quá 256 ký tự.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Mã định danh không được để trống")]
    [StringLength(100, ErrorMessage = "Mã định danh không được vượt quá 100 ký tự.")]
    public required string Sku { get; set; }

    [Required(ErrorMessage = "Danh mục không được để trống")]
    public required Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Thương hiệu không được để trống")]
    public required Guid BrandId { get; set; }

    [Required(ErrorMessage = "Giá tiền không được để trống")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 0")]
    public required decimal Price { get; set; }

    [Required(ErrorMessage = "Số tháng bảo hành không được để trống")]
    [Range(0, int.MaxValue, ErrorMessage = "Số tháng bảo hành phải lớn hơn hoặc bằng 0")]
    public required int WarrantyMonth { get; set; }

    [Required(ErrorMessage = "Mô tả không được để trống")]
    [StringLength(5000, ErrorMessage = "Mô tả không được vượt quá 5000 ký tự.")]
    public required string Description { get; set; }

    public List<string> Images { get; set; } = [];

    public List<CreateProductSpecValueRequest> SpecValues { get; set; } = [];

    [Required(ErrorMessage = "Danh sách linh kiện không được để trống")]
    [MinLength(1, ErrorMessage = "PC phải có ít nhất 1 linh kiện")]
    public List<ProductPCComponentRequest> Components { get; set; } = [];
}
