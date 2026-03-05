using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class CashPaymentRequest
{
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    public string? Note { get; set; }
}
