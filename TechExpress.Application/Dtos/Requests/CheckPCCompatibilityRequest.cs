using System;
using TechExpress.Application.DTOs.Requests;

namespace TechExpress.Application.Dtos.Requests;

public class CheckPCCompatibilityRequest
{
    public List<AddItemToCustomPCRequest> Schema { get; set; } = []; 
}
