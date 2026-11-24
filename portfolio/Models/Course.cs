namespace portfolio.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string TermTaken { get; set; } = string.Empty;
        public int Units { get; set; }
        public string? Description { get; set; }
    }
}