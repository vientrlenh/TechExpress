using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class PromotionRepository
{
    private readonly ApplicationDbContext _context;

    public PromotionRepository(ApplicationDbContext context)
    {
        _context = context; 
    }

    public async Task<Promotion?> FindByCodeAsync(string code)
    {
        return await _context.Promotions.FirstOrDefaultAsync(p => !string.IsNullOrEmpty(p.Code) && p.Code == code);
    }


}
