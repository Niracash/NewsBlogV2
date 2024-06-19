using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlog.Data;
using NewsBlog.Models;
using X.PagedList;
using System.Threading.Tasks;

namespace NewsBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _context;

        public AuditLogController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            var logs = await _context.AuditLogs.OrderByDescending(log => log.Timestamp).ToListAsync();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(logs.ToPagedList(pageNumber, pageSize));
        }
    }
}
