using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class ChatMessageRepository(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<ChatMessage>> FindChatMessagesIncludeMediasBySessionIdWithSplitQueryAsync(Guid sessionId, int size, int pageIndex)
    {
        return await _context.ChatMessages
            .Include(c => c.Medias)
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageIndex - 1) * size)
            .Take(size + 1)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task AddAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
    }

    public async Task<ChatMessage?> FindByIdIncludeMediasAsync(Guid id)
    {
        return await _context.ChatMessages.Include(c => c.Medias).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<ChatMessage>> FindRecentMessagesForAiContextAsync(Guid sessionId, int count)
    {
        var messages = await _context.ChatMessages
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();
        messages.Reverse();
        return messages;
    }
}
