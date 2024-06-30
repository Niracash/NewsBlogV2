using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsBlog.Data;
using NewsBlog.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace NewsBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _db;

        public AuditLogController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var logsQuery = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                logsQuery = logsQuery.Where(x => x.UserName.Contains(searchString) || x.Action.Contains(searchString) || x.Details.Contains(searchString));
                ViewBag.CurrentFilter = searchString;
            }

            var auditLogs = await logsQuery.OrderByDescending(x => x.Timestamp).ToListAsync();
            var viewModel = auditLogs.Select(x => new AuditLogViewModel
            {
                UserName = x.UserName,
                Action = x.Action,
                Timestamp = x.Timestamp,
                Details = x.Details
            }).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(viewModel.ToPagedList(pageNumber, pageSize));
        }
    }
}
