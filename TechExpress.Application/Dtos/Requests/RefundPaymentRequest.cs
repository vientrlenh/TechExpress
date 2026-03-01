namespace TechExpress.Application.Dtos.Requests;

/// <summary>
/// Request: refund payment.
/// </summary>
public class RefundPaymentRequest
{
    /// <summary>
    /// Lý do refund (optional).
    /// </summary>
    public string? Reason { get; set; }
}
