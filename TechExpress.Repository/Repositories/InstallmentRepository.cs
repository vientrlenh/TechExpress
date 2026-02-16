using System;
using System.Collections.Generic;
using System.Text;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class InstallmentRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public InstallmentRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Installment installment)
        {
            await _dbContext.Installments.AddAsync(installment);
        }
    }
}
