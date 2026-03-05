using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public class SetPaymentIntentRequest
{
    
    [Required]
    public PaymentMethod Method { get; set; }
}
