using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;

namespace TechExpress.Service.Contexts
{
    public class UserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentAuthenticatedUserId()
        {
            string? idStr = (_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value) ?? throw new UnauthorizedException("Người dùng chưa xác thực.");
            return Guid.Parse(idStr);
        }

        public string? GetCurrentAuthenticatedUserIdIfExist()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public UserRole? GetCurrentUserRole()
        {
            var roleStr = _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;
            if (roleStr == null) return null;
            return Enum.TryParse<UserRole>(roleStr, out var role) ? role : null;
        }
    }
}
