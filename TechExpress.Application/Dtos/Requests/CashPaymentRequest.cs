using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

/// <summary>
/// Request: staff ghi nhận thu tiền mặt/COD.
/// </summary>
public class CashPaymentRequest
{
    /// <summary>
    /// Số tiền thu.
    /// </summary>
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Ghi chú thu tiền (optional).
    /// </summary>
    public string? Note { get; set; }
}
