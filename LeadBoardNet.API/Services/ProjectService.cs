using AutoMapper;
using FluentValidation;
using LeadBoard.Shared.Dtos.Settings.Projects;
using LeadBoard.Shared.Entities;
using LeadBoard.Shared.Wrappers;
using LeadBoardNet.API.Repositories;

namespace LeadBoardNet.API.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<ProjectRequest> _validator;
    private readonly IImageManager _imageManager;

    public ProjectService(
        IProjectRepository projectRepository,
        ILogger<ProjectService> logger,
        IMapper mapper,
        IValidator<ProjectRequest> validator,
        IImageManager imageManager)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
        _imageManager = imageManager;
    }

    public async Task<Result<ProjectResponse>> CreateAsync(ProjectRequest request, IFormFile? mainImage,
        List<IFormFile>? images)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var erros = validationResult.Errors.Select(e => e.ErrorMessage);
            return Result<ProjectResponse>.BadRequest(string.Join(", ", erros));
        }

        if (await _projectRepository.GetByTitleAsync(request.Title) != null)
            return Result<ProjectResponse>.Conflict("El titulo ya existe");

        var project = _mapper.Map<Project>(request);

        string? mainPublicId = null;
        List<string> galleryPublicIds = new();

        try
        {
            if (mainImage != null)
            {
                var main = await _imageManager.UploadSingleAsync(mainImage);
                project.MainImageUrl = main.Url;
                project.MainImagePublicId = main.PublicId;
                mainPublicId = main.PublicId;
            }

            if (images?.Any() == true)
            {
                var galleryResults = await _imageManager.UploadMultipleAsync(images);
                galleryPublicIds = galleryResults.Select(x => x.PublicId).ToList();

                project.Gallery = galleryResults.Select(x => new ProjectImage
                {
                    Url = x.Url,
                    PublicId = x.PublicId,
                    Project = project
                }).ToList();
            }

            var createdProject = await _projectRepository.CreateAsync(project);

            var response = _mapper.Map<ProjectResponse>(createdProject);
            return Result<ProjectResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cr�tico al crear el proyecto. Iniciando rollback de im�genes.");

            // 6. EL MOMENTO DE LA VERDAD: Rollback
            // Si fall� el guardado en DB o cualquier cosa, borramos de Cloudinary
            await _imageManager.RollbackAsync(mainPublicId, galleryPublicIds);

            // Re-lanzamos para que el Middleware se encargue de la respuesta HTTP
            throw;
        }
    }

    public async Task<Result<ProjectResponse>> UpdateAsync(int id, ProjectRequest request,
        IFormFile? newMainImage)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return Result<ProjectResponse>.NotFound("Proyecto no encontrado");

        // Guardamos el ID viejo para borrarlo DESPU�S del �xito
        string? oldMainPublicId = null;
        string? newMainPublicId = null;

        try
        {
            _mapper.Map(request, project);

            if (newMainImage != null)
            {
                var uploadResult = await _imageManager.UploadSingleAsync(newMainImage);

                // Guardamos el ID de la vieja para borrarla luego
                oldMainPublicId = project.MainImagePublicId;

                // Actualizamos la entidad con la nueva info
                project.MainImageUrl = uploadResult.Url;
                project.MainImagePublicId = uploadResult.PublicId;

                // Guardamos el ID de la nueva por si hay que hacer rollback
                newMainPublicId = uploadResult.PublicId;
            }

            await _projectRepository.UpdateAsync(project);

            if (!string.IsNullOrEmpty(oldMainPublicId))
            {
                await _imageManager.DeleteAsync(oldMainPublicId);
            }

            return Result<ProjectResponse>.Success(_mapper.Map<ProjectResponse>(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar proyecto {Id}. Limpiando basura.", id);

            // ROLLBACK: Si alcanzamos a subir una imagen nueva pero la DB fall�, la borramos
            if (!string.IsNullOrEmpty(newMainPublicId))
            {
                await _imageManager.RollbackAsync(newMainPublicId);
            }

            throw; // Al middleware
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        // 1. Obtener el proyecto con TODO su grafo (Main Image + Gallery)
        var project = await _projectRepository.GetByIdWithGalleryAsync(id);
        if (project == null) return Result<bool>.NotFound("Proyecto no encontrado");

        // 2. Recolectar todos los PublicIds para la limpieza posterior
        var publicIdsToDelete = new List<string>();

        if (!string.IsNullOrEmpty(project.MainImagePublicId))
            publicIdsToDelete.Add(project.MainImagePublicId);

        if (project.Gallery?.Any() == true)
            publicIdsToDelete.AddRange(project.Gallery.Select(x => x.PublicId));

        try
        {
            // 3. Eliminar de la Base de Datos primero
            // Al borrar el Proyecto, EF borrar� en cascada los Tags y registros de Galer�a
            await _projectRepository.DeleteAsync(project);

            // 4. Si la DB fue exitosa, procedemos a limpiar Cloudinary
            // Usamos RollbackAsync porque ya tiene la l�gica de borrar m�ltiples IDs en paralelo
            await _imageManager.RollbackAsync(null, publicIdsToDelete);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cr�tico al eliminar el proyecto {Id}", id);
            throw; // Al Middleware
        }
    }

    public async Task<Result<bool>> DeleteGalleryImageAsync(int projectId, int imageId)
    {
        var project = await _projectRepository.GetByIdWithGalleryAsync(projectId);
        if (project == null) return Result<bool>.NotFound("Proyecto no encontrado");

        var image = project.Gallery.FirstOrDefault(x => x.Id == imageId);
        if (image == null) return Result<bool>.NotFound("La imagen no pertenece a este proyecto");

        try
        {
            await _imageManager.DeleteAsync(image.PublicId);

            project.Gallery.Remove(image);
            await _projectRepository.UpdateAsync(project);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar imagen {ImageId} del proyecto {ProjectId}", imageId, projectId);
            throw; // Al Middleware
        }
    }
}
