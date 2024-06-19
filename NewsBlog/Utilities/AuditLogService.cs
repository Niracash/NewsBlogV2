using NewsBlog.Data;
using NewsBlog.Models;

namespace NewsBlog.Utilities
{
    public class AuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string details)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            var log = new AuditLog
            {
                Action = action,
                UserName = userName,
                Timestamp = DateTime.UtcNow,
                Details = details
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}

