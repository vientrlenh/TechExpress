using System;
using TechExpress.Repository.Contexts;

namespace TechExpress.Repository.Repositories;

public class CustomPCItemRepository
{
    private readonly ApplicationDbContext _context;

    public CustomPCItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

}
