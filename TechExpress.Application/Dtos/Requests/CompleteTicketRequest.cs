using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public record CompleteTicketRequest(
    [Required] TicketStatus Status
);
