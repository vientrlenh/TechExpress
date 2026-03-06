using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Responses;

/// <summary>
/// Response: kết quả hủy đơn hàng và hoàn tiền.
/// </summary>
public class CancelOrderRefundResponse
{
    /// <summary>
    /// Order ID đã bị hủy.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng sau khi hủy.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Số tiền đã hoàn lại (90% số tiền đã thanh toán).
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Payout ID từ PayOS (nếu có).
    /// </summary>
    public string? PayoutId { get; set; }

    /// <summary>
    /// Lý do hủy đơn hàng.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Thông báo kết quả.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
