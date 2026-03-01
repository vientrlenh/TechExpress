using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

/// <summary>
/// Request: set payment intent (trả thẳng) cho order.
/// </summary>
public class SetPaymentIntentRequest
{
    /// <summary>
    /// Phương thức thanh toán dự kiến tại checkout.
    /// </summary>
    [Required]
    public PaymentMethod Method { get; set; }
}
