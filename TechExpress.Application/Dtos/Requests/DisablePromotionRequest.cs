using System;
using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public class DisablePromotionRequest
{
    [Required(ErrorMessage = "Id của khuyến mãi là bắt buộc để tắt")]
    public Guid PromotionId { get; set; }
}
