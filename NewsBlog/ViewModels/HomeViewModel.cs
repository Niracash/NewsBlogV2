using NewsBlog.Models;
using X.PagedList;

namespace NewsBlog.ViewModels
{
    public class HomeViewModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IPagedList<Post>? Posts { get; set; }
    }
}
