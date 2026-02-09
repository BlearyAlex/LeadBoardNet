using LeadBoard.Shared.Dtos.Settings.Projects;
using LeadBoard.Shared.Wrappers;

namespace LeadBoardNet.API.Services;

public interface IProjectService
{
    Task<Result<ProjectResponse>> CreateAsync(ProjectRequest request, IFormFile? mainImage, List<IFormFile>? images);
    Task<Result<ProjectResponse>> UpdateAsync(int id, ProjectRequest request, IFormFile? newMainImage);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<bool>> DeleteGalleryImageAsync(int projectId, int imageId);
}
