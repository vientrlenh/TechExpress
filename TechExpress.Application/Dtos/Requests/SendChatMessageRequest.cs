using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class SendChatMessageRequest
{
    [RegularExpression(@"^(\+84|0)[3|5|7|8|9][0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
    public required string Message { get; set; }

    public List<SendChatMediaRequest> Medias { get; set; } = [];
}
