using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class CreateCustomPCBuildRequest
{
    [Required(ErrorMessage = "Tên của cấu hình tự chọn là bắt buộc")]
    public required string Name { get; set; }
}
