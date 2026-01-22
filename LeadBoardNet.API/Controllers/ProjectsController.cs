using LeadBoard.Shared.Dtos.Request;
using LeadBoardNet.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeadBoardNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateProjectDto request,
        IFormFile? mainImage,
        List<IFormFile>? images)
    {
        var result = await _projectService.CreateAsync(request, mainImage, images);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        
        var location = $"/api/projects/{result.Value.Id}";
        
        return Created(location, result.Value);
    }
}