using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public record CreateTicketRequest(

    [MaxLength(256, ErrorMessage = "Tên đầy đủ không được vượt quá 256 ký tự")]
    string? FullName,

    [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
    string? Phone,

    [Required(ErrorMessage = "Tiêu đề của ticket là bắt buộc")] 
    [MaxLength(256, ErrorMessage = "Tiêu đề của ticket không được vượt quá 256 ký tự")]
    string Title,

    [Required(ErrorMessage = "Mô tả của ticket là bắt buộc")]
    [StringLength(4096, ErrorMessage = "Mô tả của ticket không được vượt quá 4096 ký tự")]
    string Description,

    [Required(ErrorMessage = "Tin nhắn của ticket là bắt buộc")] 
    [StringLength(4096, ErrorMessage = "Độ dài tin nhắn không được vượt quá 4096 ký tự")]
    string Message,

    [Required(ErrorMessage = "Loại ticket là bắt buộc")]
    [EnumDataType(typeof(TicketType), ErrorMessage = "Loại ticket không được hỗ trợ")]
    TicketType Type,

    Guid? CustomPCId,

    Guid? OrderId,

    long? OrderItemId,

    List<string> Attachments
);
