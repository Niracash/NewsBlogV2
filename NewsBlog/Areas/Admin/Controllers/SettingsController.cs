using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
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
    public class SettingsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuditLogService _auditLogService;

        public SettingsController(AppDbContext db, INotyfService notyfService, IWebHostEnvironment webHostEnvironment, AuditLogService auditLogService)
        {
            _db = db;
            _notification = notyfService;
            _webHostEnvironment = webHostEnvironment;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _db.Settings!.FirstOrDefaultAsync();
            if (settings == null)
            {
                return View("Error");
            }

            var settingsViewModel = new SettingsViewModel()
            {
                Id = settings.Id,
                Logo = settings.Logo,
                Title = settings.Title,
                Description = settings.Description,
                TwitterUrl = settings.TwitterUrl,
                FacebookUrl = settings.FacebookUrl,
                GithubUrl = settings.GithubUrl
            };

            return View(settingsViewModel);
        }

        // Edit
        [HttpPost]
        public async Task<IActionResult> Index(SettingsViewModel settingsViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(settingsViewModel);
            }
            var settings = await _db.Settings!.FirstOrDefaultAsync(x => x.Id == settingsViewModel.Id);
            if (settings == null)
            {
                _notification.Error("Error 404 not found");
                return View(settingsViewModel);
            }

            // Compare original and new values for logging
            var changes = new List<string>();
            if (settings.Logo != settingsViewModel.Logo)
            {
                changes.Add("Logo");
                settings.Logo = settingsViewModel.Logo;
            }
            if (settings.Title != settingsViewModel.Title)
            {
                changes.Add("Title");
                settings.Title = settingsViewModel.Title;
            }
            if (settings.Description != settingsViewModel.Description)
            {
                changes.Add("Description");
                settings.Description = settingsViewModel.Description;
            }
            if (settings.TwitterUrl != settingsViewModel.TwitterUrl)
            {
                changes.Add("TwitterUrl");
                settings.TwitterUrl = settingsViewModel.TwitterUrl;
            }
            if (settings.FacebookUrl != settingsViewModel.FacebookUrl)
            {
                changes.Add("FacebookUrl");
                settings.FacebookUrl = settingsViewModel.FacebookUrl;
            }
            if (settings.GithubUrl != settingsViewModel.GithubUrl)
            {
                changes.Add("GithubUrl");
                settings.GithubUrl = settingsViewModel.GithubUrl;
            }

            await _db.SaveChangesAsync();
            _notification.Success("Settings updated!");

            if (changes.Count > 0)
            {
                var changesString = string.Join(", ", changes);
                await _auditLogService.LogAsync("Settings Edited", $"<strong>Settings</strong> were edited by <strong>{User.Identity!.Name}</strong>. Changes: <strong>{changesString}<strong>.");
            }

            return RedirectToAction("Index", "Settings", new { area = "Admin" });
        }
    }
}
