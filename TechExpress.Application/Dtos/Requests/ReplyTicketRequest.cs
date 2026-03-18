using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests;

public record ReplyTicketRequest(
    [Required] string Content,
    List<string>? Attachments,
    string? Phone
);
