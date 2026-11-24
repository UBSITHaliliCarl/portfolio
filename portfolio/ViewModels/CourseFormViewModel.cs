using System.ComponentModel.DataAnnotations;

namespace portfolio.ViewModels
{
    public class CourseFormViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Course code is required.")]
        [StringLength(20, ErrorMessage = "Course code cannot exceed 20 characters.")]
        // Regex #1: Course code format (e.g., DBMWEB1, IT-101, NETSEC3)
        [RegularExpression(@"^[A-Z]{2,8}-?[0-9]{1,4}[A-Z0-9]?$",
            ErrorMessage = "Use uppercase letters and numbers only (e.g., DBMWEB1 or IT-101).")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course title is required.")]
        [StringLength(150, ErrorMessage = "Course title cannot exceed 150 characters.")]
        [Display(Name = "Course Title")]
        public string CourseTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Term taken is required.")]
        [StringLength(40, ErrorMessage = "Term taken cannot exceed 40 characters.")]
        [Display(Name = "Term Taken")]
        public string TermTaken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Units are required.")]
        [Range(1, 6, ErrorMessage = "Units must be between 1 and 6.")]
        public int Units { get; set; } = 3;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
    }
}