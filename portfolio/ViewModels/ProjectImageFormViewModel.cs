using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace portfolio.ViewModels
{
    public class ProjectImageFormViewModel
    {
        public int ImageId { get; set; }

        [Required(ErrorMessage = "Please select a project.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project.")]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        [RegularExpression(@"^https:\/\/[^\s""'<>]+\.(png|jpe?g|webp|gif|svg)(\?[^\s""'<>]*)?$",
            ErrorMessage = "Must be a direct HTTPS image URL ending in .png, .jpg, .jpeg, .webp, or .gif.")]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Caption cannot exceed 200 characters.")]
        public string? Caption { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Sort order must be at least 1.")]
        [Display(Name = "Display Order")]
        public int SortOrder { get; set; } = 1;

        public IEnumerable<SelectListItem> Projects { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}