using System;

namespace TechExpress.Application.Dtos.Responses;

public record CustomPCItemResponse(
    long Id,
    Guid CustomPCId,
    Guid ProductId,
    int Quantity
);

public record CustomPCResponse(
    Guid Id,
    Guid UserId,
    string Name,
    DateTimeOffset UpdatedAt,
    List<CustomPCItemResponse> Items
);
