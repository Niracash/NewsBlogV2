using System.ComponentModel.DataAnnotations;
using NewsBlog.ViewModels;

namespace NewsBlog.Attributes
{
    public class ImageOrVideoRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (CreatePostViewModel)validationContext.ObjectInstance;

            if (model.UploadImage == null && model.UploadVideo == null && string.IsNullOrEmpty(model.ImageUrl) && string.IsNullOrEmpty(model.VideoUrl))
            {
                return new ValidationResult("Either an image or a video must be uploaded.");
            }

            return ValidationResult.Success!;
        }
    }
}
