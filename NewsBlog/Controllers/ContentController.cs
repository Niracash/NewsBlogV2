using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlog.Data;
using NewsBlog.ViewModels;
using HtmlAgilityPack;


namespace NewsBlog.Controllers
{
    public class ContentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly INotyfService _notification;

        public ContentController(AppDbContext db, INotyfService notification)
        {
            _db = db;
            _notification = notification;
        }
        [HttpGet("[controller]/{slug}")]
        public IActionResult Article(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                _notification.Error("Post not found");
                return View();
            }

            var article = _db.Posts!.Include(x => x.User).FirstOrDefault(p => p.Slug == slug);
            if (article == null)
            {
                _notification.Error("Post not found");
                return View();
            }

            var viewModel = new ContentViewModel()
            {
                Id = article.Id,
                Title = article.Title,
                AuthorName = article.User != null ? article.User.FirstName + " " + article.User.LastName : "Unknown",
                CreatedAt = article.CreatedAt,
                ImageUrl = article.ImageUrl,
                Description = article.Description,
            };
            viewModel.Description = ModifyLinksToOpenInNewTab(article.Description!);
            return View(viewModel);
        }

        // Open in new tab
        private string ModifyLinksToOpenInNewTab(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return htmlContent; // Return original content if it's null or empty
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null) // Check if any links were found
            {
                foreach (var link in links)
                {
                    link.SetAttributeValue("target", "_blank");
                    link.SetAttributeValue("rel", "noopener noreferrer"); // For security reasons
                }
            }

            return doc.DocumentNode.OuterHtml;
        }
    }
}
