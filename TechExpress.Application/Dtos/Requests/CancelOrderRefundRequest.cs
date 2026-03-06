using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public sealed class CancelOrderRefundRequest
{
    [Required(ErrorMessage = "OrderId is required.")]
    public required Guid OrderId { get; set; }

    [Required(ErrorMessage = "ToBin is required.")]
    public required string ToBin { get; set; }

    [Required(ErrorMessage = "ToAccountNumber is required.")]
    public required string ToAccountNumber { get; set; }

    public string? Reason { get; set; }
}
