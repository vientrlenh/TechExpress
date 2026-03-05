using System;
using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public class CreatePromotionRequest
{
    [Required(ErrorMessage = "Tên khuyến mãi là bắt buộc")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên khuyến mãi phải từ 3 đến 200 ký tự")]
    public required string Name { get; set; }

    [StringLength(50, ErrorMessage = "Mã khuyến mãi không được vượt quá 50 ký tự")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "Mô tả khuyến mãi là bắt buộc")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Mô tả khuyến mãi phải từ 10 đến 1000 ký tự")]
    public required string Description { get; set; }

    [EnumDataType(typeof(PromotionType), ErrorMessage = "Loại khuyến mãi không hợp lệ")]
    public required PromotionType Type { get; set; }

    [EnumDataType(typeof(PromotionScope), ErrorMessage = "Phạm vi khuyến mãi không hợp lệ")]
    public required PromotionScope Scope { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0")]
    public decimal? DiscountValue { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm giá tối đa phải lớn hơn 0")]
    public decimal? MaxDiscountValue { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu không được âm")]
    public decimal? MinOrderValue { get; set; }

    public List<CreatePromotionRequiredProductRequest> RequiredProducts { get; set; } = [];

    [EnumDataType(typeof(PromotionRequiredProductLogic), ErrorMessage = "Logic sản phẩm điều kiện không hợp lệ")]
    public PromotionRequiredProductLogic? RequiredProductLogic { get; set; }

    public List<CreatePromotionFreeProductRequest> FreeProducts { get; set; } = [];

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm được chọn miễn phí phải lớn hơn 0")]
    public int? FreeItemPickCount { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? BrandId { get; set; }

    public List<Guid> AppliedProducts { get; set; } = [];

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm tối thiểu phải lớn hơn 0")]
    public int? MinAppliedQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng tối đa phải lớn hơn 0")]
    public int? MaxUsageCount { get; set; }

    public int UsageCount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng tối đa mỗi người dùng phải lớn hơn 0")]
    public int? MaxUsagePerUser { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu khuyến mãi là bắt buộc")]
    public required string StartDate { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc khuyến mãi là bắt buộc")]
    public required string EndDate { get; set; }

    public bool IsStackable { get; set; }
}
