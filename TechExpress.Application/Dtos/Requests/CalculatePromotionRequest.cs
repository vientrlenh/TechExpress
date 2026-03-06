using System;
using TechExpress.Application.DTOs.Requests;

namespace TechExpress.Application.Dtos.Requests;

public class CalculatePromotionRequest
{
    public List<string> Codes { get; set; } = [];

    public List<CheckoutItemRequest> CheckoutItems { get; set; } = [];

    public string? Phone { get; set; }
}
