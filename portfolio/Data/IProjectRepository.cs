using portfolio.Models;

namespace portfolio.Data
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();
        Task<List<Project>> GetByCourseIdAsync(int courseId);
        Task<Project?> GetByIdAsync(int projectId);
        Task<int> CreateWithImagesAsync(Project project, IReadOnlyList<string> imageUrls);
        Task<bool> UpdateWithImagesAsync(Project project, IReadOnlyList<string> imageUrls);
        Task<bool> DeleteAsync(int projectId);
    }
}