using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NewsBlog.Data;
using NewsBlog.Models;
using NewsBlog.Utilities;
using NewsBlog.ViewModels;
using X.PagedList;
using System.Linq;

namespace NewsBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PostController : Controller
    {
        private readonly AppDbContext _db;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<User> _userManager;
        private readonly AuditLogService _auditLogService;

        public PostController(AppDbContext db, INotyfService notyfService, IWebHostEnvironment webHostEnvironment, UserManager<User> userManager, AuditLogService auditLogService)
        {
            _db = db;
            _notification = notyfService;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var userRole = await _userManager.GetRolesAsync(user!);

            var postsQuery = _db.Posts!.Include(x => x.User).AsQueryable();
            if (!userRole.Contains(Roles.Admin) && !userRole.Contains(Roles.SuperAdmin))
            {
                postsQuery = postsQuery.Where(x => x.User!.Id == user!.Id);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                var searchTerms = searchString.Split(' ');

                postsQuery = postsQuery.Where(x => searchTerms.All(term =>
                    x.Title.Contains(term) ||
                    x.Description.Contains(term) ||
                    (x.User != null && x.User.FirstName.Contains(term)) ||
                    (x.User != null && x.User.LastName.Contains(term))));

                ViewBag.CurrentFilter = searchString;
            }

            var postList = await postsQuery.OrderByDescending(x => x.CreatedAt).ToListAsync();

            var postListViewModel = postList.Select(x => new PostListViewModel()
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl,
                VideoUrl = x.VideoUrl,
                Title = x.Title,
                Description = x.Description,
                AuthorName = x.User != null ? x.User.FirstName + " " + x.User.LastName : x.AuthorName ?? "Unknown Author",
                CreatedAt = x.CreatedAt,
                Slug = x.Slug  // Ensure the Slug is mapped
            }).ToList();

            int pageSize = 6;
            int pageNumber = (page ?? 1);
            return View(await postListViewModel.ToPagedListAsync(pageNumber, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _db.Posts!.FirstOrDefaultAsync(x => x.Id == id);
            if (post == null)
            {
                _notification.Error("Post not found");
                return View();
            }
            // logged in user
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var userRole = await _userManager.GetRolesAsync(user!);
            if (!userRole.Contains(Roles.Admin) && !userRole.Contains(Roles.SuperAdmin) && user!.Id != post.UserId)
            {
                _notification.Error("This is not your post!");
                return RedirectToAction("Index");
            }

            var viewModel = new CreatePostViewModel()
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                ImageUrl = post.ImageUrl,
                VideoUrl = post.VideoUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreatePostViewModel createPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(createPostViewModel);
            }

            var post = await _db.Posts!.FirstOrDefaultAsync(x => x.Id == createPostViewModel.Id);
            if (post == null)
            {
                _notification.Error("Post not found");
                return View();
            }

            var changes = new List<string>();
            if (post.Title != createPostViewModel.Title)
            {
                changes.Add("Title");
                post.Title = createPostViewModel.Title;
            }
            if (post.Description != createPostViewModel.Description)
            {
                changes.Add("Description");
                post.Description = createPostViewModel.Description;
            }

            if (createPostViewModel.UploadImage != null)
            {
                changes.Add("Image");
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", post.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                post.ImageUrl = SaveFile(createPostViewModel.UploadImage, "images");
            }
            else if (string.IsNullOrEmpty(createPostViewModel.ImageUrl))
            {
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", post.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    changes.Add("Image");
                    post.ImageUrl = null;
                }
            }
            else
            {
                post.ImageUrl = createPostViewModel.ImageUrl;
            }

            if (createPostViewModel.UploadVideo != null)
            {
                changes.Add("Video");
                if (!string.IsNullOrEmpty(post.VideoUrl))
                {
                    var oldVideoPath = Path.Combine(_webHostEnvironment.WebRootPath, "videos", post.VideoUrl);
                    if (System.IO.File.Exists(oldVideoPath))
                    {
                        System.IO.File.Delete(oldVideoPath);
                    }
                }
                post.VideoUrl = SaveFile(createPostViewModel.UploadVideo, "videos");
            }
            else if (string.IsNullOrEmpty(createPostViewModel.VideoUrl))
            {
                if (!string.IsNullOrEmpty(post.VideoUrl))
                {
                    var oldVideoPath = Path.Combine(_webHostEnvironment.WebRootPath, "videos", post.VideoUrl);
                    if (System.IO.File.Exists(oldVideoPath))
                    {
                        System.IO.File.Delete(oldVideoPath);
                    }
                    changes.Add("Video");
                    post.VideoUrl = null;
                }
            }
            else
            {
                post.VideoUrl = createPostViewModel.VideoUrl;
            }

            await _db.SaveChangesAsync();
            _notification.Success("Post updated!");

            if (changes.Count > 0)
            {
                var changesString = string.Join(", ", changes);
                await _auditLogService.LogAsync("Post Edited", $"Post <strong>{post.Title}</strong> was edited by <strong>{User.Identity!.Name}</strong>. Changes: <strong>{changesString}</strong>.");
            }

            return RedirectToAction("Index", "Post", new { area = "Admin" });
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel createPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(createPostViewModel);
            }

            // get logged in user's id and name
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);

            var post = new Post
            {
                Title = createPostViewModel.Title,
                Description = createPostViewModel.Description,
                UserId = user!.Id,
                AuthorName = user.FirstName + " " + user.LastName, // Set AuthorName
            };

            if (post.Title != null)
            {
                string slug = createPostViewModel.Title!.Trim();
                slug = slug.Replace(" ", "-");
                post.Slug = slug + "-" + Guid.NewGuid();
            }

            if (createPostViewModel.UploadImage != null)
            {
                post.ImageUrl = SaveFile(createPostViewModel.UploadImage, "images");
            }

            if (createPostViewModel.UploadVideo != null)
            {
                post.VideoUrl = SaveFile(createPostViewModel.UploadVideo, "videos");
            }

            await _db.Posts!.AddAsync(post);
            await _db.SaveChangesAsync();
            _notification.Success("Blog created successfully");
            await _auditLogService.LogAsync("Post Created", $"Post <strong>{post.Title}</strong> was created by <strong>{User.Identity!.Name}</strong>.");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var autherPost = await _db.Posts!.FirstOrDefaultAsync(x => x.Id == id);
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var userRole = await _userManager.GetRolesAsync(user!);
            if (userRole.Contains(Roles.Admin) || userRole.Contains(Roles.SuperAdmin) || user?.Id == autherPost?.UserId)
            {
                // Delete the image from the server
                if (!string.IsNullOrEmpty(autherPost!.ImageUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", autherPost.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                if (!string.IsNullOrEmpty(autherPost.VideoUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "videos", autherPost.VideoUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _db.Posts!.Remove(autherPost!);
                await _db.SaveChangesAsync();
                _notification.Success("Post have been deleted");
                await _auditLogService.LogAsync("Post Deleted", $"Post <strong>{autherPost.Title}</strong> was deleted by <strong>{User.Identity!.Name}</strong>.");

                return RedirectToAction("Index", "Post", new { area = "Admin" });
            }
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostViewModel());
        }

        private string SaveFile(IFormFile file, string folderName)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, folderName);
            var filePath = Path.Combine(folderPath, uniqueFileName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return uniqueFileName;
        }
    }
}
