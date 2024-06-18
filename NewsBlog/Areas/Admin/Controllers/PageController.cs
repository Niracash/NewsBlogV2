using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlog.Data;
using NewsBlog.Models;
using NewsBlog.Utilities;
using NewsBlog.ViewModels;

namespace NewsBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles= "Admin")]
    public class PageController : Controller
    {
        private readonly AppDbContext _db;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PageController(AppDbContext db, INotyfService notification, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _notification = notification;
            _webHostEnvironment = webHostEnvironment;
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
            if(aboutPage == null)
            {
                _notification.Error("Page not found");
                return View();
            }
            aboutPage.Title = pageViewModel.Title;
            aboutPage.Description = pageViewModel.Description;

            await _db.SaveChangesAsync();
            _notification.Success("Page updated!");
            return RedirectToAction("About", "Page", new { area = "Admin" });
        }
        [HttpGet]
        public async Task<IActionResult> Contact()
        {
            var aboutPage = await _db.Pages!.FirstOrDefaultAsync(x => x.Slug == "contact");
            var pageViewModel = new PageViewModel()
            {
                Id = aboutPage!.Id,
                Title = aboutPage.Title,
                Description = aboutPage.Description,
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
            contactPage.Title = pageViewModel.Title;
            contactPage.Description = pageViewModel.Description;


            await _db.SaveChangesAsync();
            _notification.Success("Page updated!");
            return RedirectToAction("Contact", "Page", new { area = "Admin" });
        }

    }
}
