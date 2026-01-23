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

    public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger, IMapper mapper,
        ICloudinaryService cloudinaryService)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ProjectResponseDto>> CreateAsync(CreateProjectDto request, IFormFile? mainImage,
        List<IFormFile>? images)
    {
        _logger.LogInformation("Creando nuevo proyecto: {Title}", request.Title);

        var existingProject = await _projectRepository.GetByTitleAsync(request.Title);
        if (existingProject != null)
        {
            return Result<ProjectResponseDto>.Conflict(
                $"Ya existe un proyecto con el titulo '{request.Title}'"
            );
        }

        var project = _mapper.Map<Project>(request);

        if (mainImage != null)
        {
            var uploadResult = await _cloudinaryService.UploadAsync(mainImage);

            if (!uploadResult.IsSuccess)
            {
                return Result<ProjectResponseDto>.Failure(
                    uploadResult.Error,
                    uploadResult.StatusCode
                );
            }

            project.MainImageUrl = uploadResult.Value!.Url;
            project.MainImagePublicId = uploadResult.Value.PublicId;
        }

        if (images != null && images.Any())
        {
            var galleryResult = await UploadGalleryImagesAsync(images, project);

            if (!galleryResult.IsSuccess)
            {
                // Rollback de imagen principal
                if (!string.IsNullOrEmpty(project.MainImagePublicId))
                {
                    await _cloudinaryService.DeleteAsync(project.MainImagePublicId);
                }

                return Result<ProjectResponseDto>.Failure(
                    galleryResult.Error,
                    galleryResult.StatusCode
                );
            }

            project.Gallery = galleryResult.Value;
        }

        try
        {
            var savedProject = await _projectRepository.CreateAsync(project);

            _logger.LogInformation("Proyecto creado exitosamente: {ProjectId}", savedProject.Id);

            var responseDto = _mapper.Map<ProjectResponseDto>(savedProject);
            return Result<ProjectResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar proyecto en BD. Realizando rollback de imágenes");

            await RollbackImagesAsync(project.MainImagePublicId, project.Gallery);

            // Dejar que el middleware maneje esto
            throw;
        }
    }

    private async Task<Result<List<ProjectImage>>> UploadGalleryImagesAsync(List<IFormFile> images, Project project)
    {
        var gallery = new List<ProjectImage>();
        var uploadedPublicIds = new List<string>();

        foreach (var file in images)
        {
            var result = await _cloudinaryService.UploadAsync(file);

            if (!result.IsSuccess)
            {
                await RollbackGalleryImagesAsync(uploadedPublicIds);

                return Result<List<ProjectImage>>.Failure(
                    $"Error al subir imagen de galería: {result.Error}",
                    result.StatusCode
                );
            }

            var image = new ProjectImage
            {
                Url = result.Value!.Url,
                PublicId = result.Value.PublicId,
                ProjectId = project.Id
            };

            gallery.Add(image);
            uploadedPublicIds.Add(image.PublicId);
        }

        return Result<List<ProjectImage>>.Success(gallery);
    }

    private async Task RollbackGalleryImagesAsync(List<string> publicIds)
    {
        foreach (var publicId in publicIds)
        {
            try
            {
                await _cloudinaryService.DeleteAsync(publicId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar imagen en rollback: {PublicId}", publicId);
            }
        }
    }

    private async Task RollbackImagesAsync(string? mainImagePublicId, IEnumerable<ProjectImage>? gallery)
    {
        if (!string.IsNullOrEmpty(mainImagePublicId))
        {
            try
            {
                await _cloudinaryService.DeleteAsync(mainImagePublicId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar imagen principal en rollback");
            }
        }

        if (gallery != null && gallery.Any())
        {
            var publicIds = gallery.Select(g => g.PublicId).ToList();
            await RollbackGalleryImagesAsync(publicIds);
        }
    }
}