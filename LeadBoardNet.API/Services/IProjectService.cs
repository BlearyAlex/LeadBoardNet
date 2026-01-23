using LeadBoard.Shared.Dtos.Request;
using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Entities;
using LeadBoard.Shared.Wrappers;

namespace LeadBoardNet.API.Services;

public interface IProjectService
{
    Task<Result<ProjectResponseDto>> CreateAsync(CreateProjectDto request, IFormFile? mainImage, List<IFormFile>? images);
}