using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindUserByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> FindUserByIdWithTrackingAsync(Guid id)
        {
            return await _context.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> FindUserByIdWithNoTrackingAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> FindUserByPhoneAsync(string phone)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> FindUserByRoleAsync(UserRole role)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Role == role);
        }

        public async Task<bool> UserExistByRoleAsync(UserRole role)
        {
            return await _context.Users.AnyAsync(u => u.Role == role);
        }

        public async Task<bool> UserExistByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistByPhoneAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone);
        }

        public async Task<bool> AnyAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.AnyAsync(predicate);
        }


        public async Task<User?> FindUserByEmailWithTrackingAsync(string email)
        {
            return await _context.Users.AsTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsByIdAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<(List<User> Users, int TotalCount)> GetUsersPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Users.AsNoTracking();

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        // ============= STAFF LIST =================
        private IQueryable<User> BuildStaffQuery()
        {
            return _context.Users.Where(u => u.Role == UserRole.Staff);
        }

        private async Task<List<User>> ExecutePagedStaffQueryAsync(IQueryable<User> query, int page, int pageSize)
        {
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<User>> FindStaffsSortByEmailAsync(int page, int pageSize)
        {
            var query = BuildStaffQuery().OrderBy(u => u.Email);
            return await ExecutePagedStaffQueryAsync(query, page, pageSize);
        }

        public async Task<List<User>> FindStaffsSortByFirstNameAsync(int page, int pageSize)
        {
            var query = BuildStaffQuery().OrderBy(u => u.FirstName);
            return await ExecutePagedStaffQueryAsync(query, page, pageSize);
        }

        public async Task<List<User>> FindStaffsSortBySalaryAsync(int page, int pageSize)
        {
            var query = BuildStaffQuery().OrderBy(u => u.Salary);
            return await ExecutePagedStaffQueryAsync(query, page, pageSize);
        }

        public async Task<int> GetTotalStaffCountAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Staff)
                .CountAsync();
        }

        public async Task<bool> UserExistByIdentityAsync(string identity)
        {
            return await _context.Users.AnyAsync(u => u.Identity == identity);
        }

        public async Task<List<User>> FindCustomerUsersAsync(int page, int pageSize)
        {
            return await _context.Users.Where(u => u.Role == UserRole.Customer)
                                        .OrderByDescending(c => c.CreatedAt)
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();
        }

        public async Task<int> CountCustomerUsersAsync()
        {
            return await _context.Users.Where(u => u.Role == UserRole.Customer).CountAsync();
        }

        public async Task<List<User>> FindAllCustomersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Customer)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<User>> FindAdminUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Admin && u.Status == UserStatus.Active)
                .ToListAsync();
        }
    }
}
