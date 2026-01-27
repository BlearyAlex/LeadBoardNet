using LeadBoard.Shared.Dtos;
using LeadBoard.Shared.Wrappers;
using LeadBoardNet.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeadBoardNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ApiControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> Create(
        [FromForm] ProjectRequest request,
        IFormFile? mainImage,
        List<IFormFile>? images)
    {
        var result = await _projectService.CreateAsync(request, mainImage, images);
        return HandleResult(result);
    }
}
