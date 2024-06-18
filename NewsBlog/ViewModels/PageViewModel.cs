using System.ComponentModel.DataAnnotations;

namespace NewsBlog.ViewModels
{
    public class PageViewModel
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
