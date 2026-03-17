using System.ComponentModel.DataAnnotations;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests;

public record UpdateTicketStatusRequest(
    [Required] TicketStatus Status
);
