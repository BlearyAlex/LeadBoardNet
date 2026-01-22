using AutoMapper;
using LeadBoard.Shared.Dtos.Request;
using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Entities;
using LeadBoard.Shared.Wrappers;
using LeadBoardNet.API.Repositories;

namespace LeadBoardNet.API.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ProjectResponseDto>> CreateAsync(CreateProjectDto request, IFormFile mainImage, List<IFormFile> images)
    {
        _logger.LogInformation("Creando nuevo proyecto: {Title}", request.Title);

        // 1. Mapeo de DTO a Entidad (Usando AutoMapper o mapeo manual)
        var project = _mapper.Map<Project>(request);

        // 2. Subir imagen principal
        if (mainImage != null && mainImage.Length > 0)
        {
            var result = await _cloudinaryService.UploadAsync(mainImage);
            project.MainImageUrl = result.Url;
            project.MainImagePublicId = result.PublicId;
        }
        
        // 3. Subir galería
        if (images != null && images.Any())
        {
            project.Gallery = new List<ProjectImage>();

            foreach (var file in images)
            {
                var result = await _cloudinaryService.UploadAsync(file);

                var image = new ProjectImage
                {
                    Url = result.Url,
                    PublicId = result.PublicId,
                    Project = project
                };
                
                project.Gallery.Add(image);
            }
        }
        
        // 4. Guardar en Base de Datos
        // En .NET, Add y SaveChangesAsync manejan la transacción
        var savedProject = await _projectRepository.CreateAsync(project);
        
        var responseDto = _mapper.Map<ProjectResponseDto>(savedProject);
        
        return Result<ProjectResponseDto>.Success(responseDto);
    }
}