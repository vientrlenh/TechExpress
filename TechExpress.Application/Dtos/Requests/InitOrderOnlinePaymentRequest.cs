using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

/// <summary>
/// Request: init thanh toán online cho order (full).
/// </summary>
public class InitOrderOnlinePaymentRequest
{
    /// <summary>
    /// Cổng thanh toán online: PayOs hoặc VnPay.
    /// </summary>
    [Required]
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// URL client muốn nhận kết quả (optional). Nếu null thì backend dùng default return url.
    /// </summary>
    public string? ReturnUrl { get; set; }
}
