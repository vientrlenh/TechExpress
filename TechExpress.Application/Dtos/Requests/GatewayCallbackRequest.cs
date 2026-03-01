using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

/// <summary>
/// DTO demo cho callback/return.
/// Thực tế PayOS/VnPay có payload khác nhau, PaymentService sẽ parse/verify theo provider.
/// </summary>
public class GatewayCallbackRequest
{
    /// <summary>
    /// SessionId được trả từ endpoint init online (Redis session key).
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>
    /// Gateway báo thành công hay thất bại.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Số tiền thực tế gateway báo đã thanh toán.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Chữ ký/checksum (tùy gateway). Backend phải verify.
    /// </summary>
    [Required]
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Raw payload (optional) để debug/log hoặc trường hợp gateway trả nhiều field.
    /// </summary>
    public string? Raw { get; set; }
}
