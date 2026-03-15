using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public record CreateCustomPCBuildTicketRequest(
    string? FullName,
    string? Phone,
    [Required] string Title,
    [Required] string Message,
    Guid? CustomPCId,
    List<string>? Attachments
);
