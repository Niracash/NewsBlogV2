using System.ComponentModel.DataAnnotations;

namespace NewsBlog.ViewModels
{
    public class PasswordResetViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        [Required]
        public string? NewPassword { get; set; }
        [Compare(nameof(NewPassword))]
        [Required]
        public string? RepeatPassword { get; set; }
    }
}
