using System;
using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public class SendChatMediaRequest
{
    [Required(ErrorMessage = "Đường dẫn phương tiện là bắt buộc")]
    public required string MediaUrl { get; set; }

    [Required(ErrorMessage = "Định dạng phương tiện là bắt buộc")]
    [EnumDataType(typeof(ChatMediaType), ErrorMessage = "Định dạng phương tiện không được hỗ trợ")]
    public required ChatMediaType Type { get; set; }
}
