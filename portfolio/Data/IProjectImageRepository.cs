using portfolio.Models;

namespace portfolio.Data
{
    public interface IProjectImageRepository
    {
        Task<List<ProjectImage>> GetAllAsync();
        Task<List<ProjectImage>> GetByProjectIdAsync(int projectId);
        Task<ProjectImage?> GetByIdAsync(int imageId);
        Task<int> CreateAsync(ProjectImage image);
        Task<bool> UpdateAsync(ProjectImage image);
        Task<bool> DeleteAsync(int imageId);
    }
}