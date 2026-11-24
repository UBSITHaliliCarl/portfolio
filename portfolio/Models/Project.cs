namespace portfolio.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? RepoUrl { get; set; }
        public string? VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProjectImage> Images { get; set; } = new();
    }
}