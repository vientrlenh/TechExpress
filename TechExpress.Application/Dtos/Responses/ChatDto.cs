using System;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Application.Dtos.Responses;

public record ChatSessionResponse(
    Guid Id,
    Guid? UserId,
    string? FullName,
    string? Phone,
    bool IsClosed,
    bool IsEscalated,
    DateTimeOffset UpdatedAt
);

public record ChatMessageResponse(
    Guid Id,
    Guid? SentById,
    string? SentByFullName,
    string Message,
    bool IsAiMessage,
    List<ChatMediaResponse> Medias,
    DateTimeOffset CreatedAt
);

public record ChatMessageResponseList(
    List<ChatMessageResponse> ChatMessages,
    int CurrentPage,
    bool IsMore
);

public record ChatMediaResponse(
    string MediaUrl,
    ChatMediaType Type
);
