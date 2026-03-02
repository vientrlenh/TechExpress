using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

/// <summary>
/// Request: hủy đơn hàng và hoàn tiền.
/// </summary>
public sealed class CancelOrderRefundRequest
{
    /// <summary>
    /// Order ID cần hủy.
    /// </summary>
    [Required]
    public required Guid OrderId { get; set; }

    /// <summary>
    /// Thông tin nhận tiền của khách - BIN (Bank Identification Number).
    /// </summary>
    [Required]
    public required string ToBin { get; set; }

    /// <summary>
    /// Thông tin nhận tiền của khách - Số tài khoản.
    /// </summary>
    [Required]
    public required string ToAccountNumber { get; set; }

    /// <summary>
    /// Lý do hủy đơn hàng (optional).
    /// </summary>
    public string? Reason { get; set; }
}
