using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public record CreateTicketRequest(
    [Required] string Title,
    [Required] string Message,
    TicketType Type,
    Guid? CustomPCId,
    List<string>? Attachments
);
