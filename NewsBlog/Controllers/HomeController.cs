using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlog.Data;
using NewsBlog.Models;
using NewsBlog.ViewModels;
using System.Diagnostics;
using X.PagedList;

namespace NewsBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;



        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var homeViewModel = new HomeViewModel();
            var settings = _db.Settings!.ToList();

            // Data from post
            homeViewModel.Title = settings[0].Title;
            homeViewModel.Description = settings[0].Description;
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            homeViewModel.Posts = await _db.Posts!.Include(x =>x.User).OrderByDescending(x => x.CreatedAt).ToPagedListAsync(pageNumber, pageSize);

            return View(homeViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}