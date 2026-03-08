using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class CreateChatSessionRequest
{
    public string? FullName { get; set; }

    [RegularExpression(@"^(\+84|0)[3|5|7|8|9][0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
}
