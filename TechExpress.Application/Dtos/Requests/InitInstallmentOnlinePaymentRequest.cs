using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public class InitInstallmentOnlinePaymentRequest
{
    [Required(ErrorMessage = "Method is required.")]
    public PaymentMethod Method { get; set; }

    public string? ReturnUrl { get; set; }

    public string? CancelUrl { get; set; }
}
