using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace portfolio.ViewModels
{
    public class ProjectFormViewModel : IValidatableObject
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Please select a course.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid course.")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Project title is required.")]
        [StringLength(150, ErrorMessage = "Project title cannot exceed 150 characters.")]
        [Display(Name = "Project Title")]
        public string ProjectTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Summary is required.")]
        [StringLength(2000, ErrorMessage = "Summary cannot exceed 2000 characters.")]
        public string Summary { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Repository URL cannot exceed 300 characters.")]
        // Regex #2: Public HTTP/HTTPS repository URL
        [RegularExpression(@"^https:\/\/(www\.)?(github\.com|gitlab\.com|bitbucket\.org)\/[A-Za-z0-9_.-]+\/[A-Za-z0-9_.-]+\/?$",
            ErrorMessage = "Enter a valid HTTPS GitHub, GitLab, or Bitbucket repository URL.")]
        [Display(Name = "Repository URL (Optional)")]
        public string? RepoUrl { get; set; }

        [StringLength(300, ErrorMessage = "Video URL cannot exceed 300 characters.")]
        // Regex #3: YouTube video URL
        [RegularExpression(@"^https?:\/\/(www\.)?(youtube\.com\/(watch\?v=|shorts\/|embed\/)|youtu\.be\/)[A-Za-z0-9_-]{11}([?&].*)?$",
            ErrorMessage = "Enter a valid YouTube link (watch, youtu.be, shorts, or embed).")]
        [Display(Name = "YouTube Video URL (Optional)")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Image URLs (one public URL per line)")]
        public string? ImageUrlsText { get; set; }

        public IEnumerable<SelectListItem> Courses { get; set; } = Enumerable.Empty<SelectListItem>();

        public List<string> ParseImageUrls()
        {
            if (string.IsNullOrWhiteSpace(ImageUrlsText))
                return new List<string>();

            return ImageUrlsText
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .ToList();
        }

        // Server-side line-by-line regex check (Regex #4)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            const string imagePattern = @"^https:\/\/[^\s""'<>]+\.(png|jpe?g|webp|gif|svg)(\?[^\s""'<>]*)?$";
            var lines = ParseImageUrls();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > 500)
                {
                    yield return new ValidationResult(
                        $"Line {i + 1}: Image URL exceeds 500 characters.",
                        new[] { nameof(ImageUrlsText) });
                }

                if (!Regex.IsMatch(lines[i], imagePattern, RegexOptions.IgnoreCase))
                {
                    yield return new ValidationResult(
                        $"Line {i + 1} is not a valid direct HTTPS image URL (.png, .jpg, .jpeg, .webp, .gif): {lines[i]}",
                        new[] { nameof(ImageUrlsText) });
                }
            }
        }
    }
}