using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

/// <summary>
/// Request: set installment intent và tạo schedule theo số tháng.
/// </summary>
public class SetInstallmentIntentRequest
{
    /// <summary>
    /// Số tháng trả góp (1..60).
    /// </summary>
    [Range(1, 60)]
    public int Months { get; set; }
}
