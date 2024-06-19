using Microsoft.AspNetCore.Identity;
using NewsBlog.Data;
using NewsBlog.Models;
using System.Linq;
using System.Collections.Generic;

namespace NewsBlog.Utilities
{
    public class DataInitializer : IDataInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DataInitializer(AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public void Initialize()
        {
            // It checks if the role is not SuperAdmin
            if (!_roleManager.RoleExistsAsync(Roles.SuperAdmin!).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin!)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Roles.Admin!)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Roles.Author!)).GetAwaiter().GetResult();

                _userManager.CreateAsync(new User()
                {
                    UserName = "SuperAdmin",
                    Email = "superadmin@admin.com",
                    FirstName = "Super",
                    LastName = "Admin",
                }, "Passw0rd.").Wait();

                var newUser = _context.Users!.FirstOrDefault(x => x.Email == "superadmin@admin.com");
                if (newUser != null)
                {
                    _userManager.AddToRoleAsync(newUser, Roles.SuperAdmin!).GetAwaiter().GetResult();
                }

                var listPages = new List<Page>() {
                    new Page()
                    {
                        Title = "Weather",
                        Slug = "weather"
                    },
                    new Page()
                    {
                        Title = "About Us",
                        Slug = "about"
                    },
                    new Page()
                    {
                        Title = "Contact Us",
                        Slug = "contact"
                    },
                };
                _context.Pages!.AddRangeAsync(listPages);
                _context.SaveChanges();

                InitializeSettings();
            }
        }

        private void InitializeSettings()
        {
            if (!_context.Settings!.Any())
            {
                var defaultSettings = new Settings
                {
                    Logo = "LOGO Text",
                    Title = "Title",
                    Description = "Description of News Blog",
                };

                _context.Settings!.Add(defaultSettings);
                _context.SaveChanges();
            }
        }

        private void CleanUpUnusedFiles()
        {
            var imageFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            var videoFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "videos");

            var allImages = Directory.GetFiles(imageFolderPath);
            var allVideos = Directory.GetFiles(videoFolderPath);

            var usedImages = _context.Posts.Select(p => p.ImageUrl).Where(url => !string.IsNullOrEmpty(url)).ToList();
            var usedVideos = _context.Posts.Select(p => p.VideoUrl).Where(url => !string.IsNullOrEmpty(url)).ToList();

            foreach (var image in allImages)
            {
                var fileName = Path.GetFileName(image);
                if (!usedImages.Contains(fileName))
                {
                    System.IO.File.Delete(image);
                }
            }

            foreach (var video in allVideos)
            {
                var fileName = Path.GetFileName(video);
                if (!usedVideos.Contains(fileName))
                {
                    System.IO.File.Delete(video);
                }
            }
        }
    }
}
