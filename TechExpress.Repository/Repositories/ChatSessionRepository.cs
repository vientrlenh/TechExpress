using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class ChatSessionRepository(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ChatSession?> FindByUserIdAndNotClosedAsync(Guid userId)
    {
        return await _context.ChatSessions.FirstOrDefaultAsync(c => c.UserId == userId && !c.IsClosed);
    }

    public async Task<ChatSession?> FindByPhoneAndNotClosedAsync(string phone)
    {
        return await _context.ChatSessions.FirstOrDefaultAsync(c => c.Phone == phone && !c.IsClosed);
    }

    public async Task AddAsync(ChatSession session)
    {
        await _context.ChatSessions.AddAsync(session);
    }

    public async Task<ChatSession?> FindByIdAsync(Guid id)
    {
        return await _context.ChatSessions.FirstOrDefaultAsync(c => c.Id == id);
    }
}
