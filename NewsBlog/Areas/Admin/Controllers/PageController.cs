using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlog.Data;
using NewsBlog.Models;
using NewsBlog.Utilities;
using NewsBlog.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class PageController : Controller
    {
        private readonly AppDbContext _db;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuditLogService _auditLogService;

        public PageController(AppDbContext db, INotyfService notification, IWebHostEnvironment webHostEnvironment, AuditLogService auditLogService)
        {
            _db = db;
            _notification = notification;
            _webHostEnvironment = webHostEnvironment;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> About()
        {
            var aboutPage = await _db.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
            var pageViewModel = new PageViewModel()
            {
                Id = aboutPage!.Id,
                Title = aboutPage.Title,
                Description = aboutPage.Description,
            };

            return View(pageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> About(PageViewModel pageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(pageViewModel);
            }

            var aboutPage = await _db.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
            if (aboutPage == null)
            {
                _notification.Error("Page not found");
                return View();
            }

            // Compare original and new values for logging
            var changes = new List<string>();
            if (aboutPage.Title != pageViewModel.Title)
            {
                changes.Add("Title");
                aboutPage.Title = pageViewModel.Title;
            }
            if (aboutPage.Description != pageViewModel.Description)
            {
                changes.Add("Description");
                aboutPage.Description = pageViewModel.Description;
            }

            await _db.SaveChangesAsync();
            _notification.Success("Page updated!");

            if (changes.Count > 0)
            {
                var changesString = string.Join(", ", changes);
                await _auditLogService.LogAsync("Page Edited", $"Page <strong>About</strong> was edited by <strong>{User.Identity!.Name}</strong>. Changes: <strong>{changesString}<strong>.");
            }

            return RedirectToAction("About", "Page", new { area = "Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> Contact()
        {
            var contactPage = await _db.Pages!.FirstOrDefaultAsync(x => x.Slug == "contact");
            var pageViewModel = new PageViewModel()
            {
                Id = contactPage!.Id,
                Title = contactPage.Title,
                Description = contactPage.Description,
            };

            return View(pageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Contact(PageViewModel pageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(pageViewModel);
            }

            var contactPage = await _db.Pages!.FirstOrDefaultAsync(x => x.Slug == "contact");
            if (contactPage == null)
            {
                _notification.Error("Page not found");
                return View();
            }

            // Compare original and new values for logging
            var changes = new List<string>();
            if (contactPage.Title != pageViewModel.Title)
            {
                changes.Add("Title");
                contactPage.Title = pageViewModel.Title;
            }
            if (contactPage.Description != pageViewModel.Description)
            {
                changes.Add("Description");
                contactPage.Description = pageViewModel.Description;
            }

            await _db.SaveChangesAsync();
            _notification.Success("Page updated!");

            if (changes.Count > 0)
            {
                var changesString = string.Join(", ", changes);
                await _auditLogService.LogAsync("Page Edited", $"Page <strong>Contact</strong> was edited by <strong>{User.Identity!.Name}</strong>. Changes: <strong>{changesString}<strong>.");
            }

            return RedirectToAction("Contact", "Page", new { area = "Admin" });
        }
    }
}
