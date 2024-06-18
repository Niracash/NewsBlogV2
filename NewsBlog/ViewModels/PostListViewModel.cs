using NewsBlog.Models;

namespace NewsBlog.ViewModels
{
    public class PostListViewModel
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
