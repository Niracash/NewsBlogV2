using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NewsBlog.Data;
using NewsBlog.Models;
using NewsBlog.ViewModels;

namespace NewsBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SettingsController(AppDbContext db, INotyfService notyfService, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _notification = notyfService;
            _webHostEnvironment = webHostEnvironment;
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
            if(!ModelState.IsValid)
            {
                return View(settingsViewModel);
            }
            var settings = await _db.Settings!.FirstOrDefaultAsync(x => x.Id == settingsViewModel.Id);
            if (settings == null)
            {
                _notification.Error("Error 404 not found");
                return View(settingsViewModel);
            }
            settings.Logo = settingsViewModel.Logo;
            settings.Title = settingsViewModel.Title;
            settings.Description = settingsViewModel.Description;
            settings.TwitterUrl = settingsViewModel.TwitterUrl;
            settings.FacebookUrl = settingsViewModel.FacebookUrl;
            settings.GithubUrl = settingsViewModel.GithubUrl;

            await _db.SaveChangesAsync();
            _notification.Success("Settings updated!");
            return RedirectToAction("Index", "Settings", new { area = "Admin" });
        }

    }
}
