using LeadBoard.Shared.Entities;

namespace LeadBoardNet.API.Repositories;

public interface IProjectRepository
{
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<Project?> GetByIdAsync(long id);
    Task<Project?> GetByIdWithGalleryAsync(long id);
    Task<Project?> GetByTitleAsync(string title);
    Task DeleteAsync(Project project);
}