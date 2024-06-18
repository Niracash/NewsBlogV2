using Microsoft.AspNetCore.Identity;
using NewsBlog.Data;
using NewsBlog.Models;

namespace NewsBlog.Utilities
{
    public class DataInitializer : IDataInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DataInitializer(AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            // It checks if the role is not Admin
            if (!_roleManager.RoleExistsAsync(Roles.Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Roles.Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Roles.Author)).GetAwaiter().GetResult();
                _userManager.CreateAsync(new User()
                {
                    UserName = "Admin",
                    Email = "admin@admin.com",
                    FirstName = "Super",
                    LastName = "Admin",
                },"Passw0rd.").Wait();

                // Retrieve the ApplicationUser with email "admin@gmail.com" from the database.
                // If the ApplicationUser exists, assign the "Admin" role to it using the UserManager.
                var newUser = _context.Users!.FirstOrDefault(x => x.Email == "admin@admin.com");
                if (newUser != null)
                {
                    _userManager.AddToRoleAsync(newUser, Roles.Admin).GetAwaiter().GetResult();
                }

                //var aboutPage = new Page()
                //{
                //    Title = "About Us",
                //    Slug = "about"
                //};

                //var contactPage = new Page()
                //{
                //    Title = "Contact Us",
                //    Slug = "contact"
                //};

                //_context.Pages.Add(aboutPage);
                //_context.Pages.Add(contactPage);
                //_context.SaveChanges();

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
                    // Set other default values or leave them as null/empty
                };

                _context.Settings!.Add(defaultSettings);
                _context.SaveChanges();
            }
        }
        
    }
}
