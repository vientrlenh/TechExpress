using System;

namespace TechExpress.Application.Dtos.Responses;

public record CustomPCItemResponse(
    long Id,
    Guid CustomPCId,
    Guid ProductId,
    Guid CategoryId,
    string ProductName,
    decimal Price,
    int WarrantyMonth,
    int Quantity,
    string? FirstImageUrl
);

public record CustomPCResponse(
    Guid Id,
    Guid? UserId,
    string? SessionId,
    string Name,
    DateTimeOffset UpdatedAt,
    List<CustomPCItemResponse> Items
);

public record CustomPCResponseList(
    Guid Id,
    Guid? UserId,
    string? SessionId,
    string Name,
    DateTimeOffset UpdatedAt
);
