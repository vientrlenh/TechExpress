using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;


public class SetInstallmentIntentRequest
{
    [Range(1, 60)]
    public int Months { get; set; }
}
