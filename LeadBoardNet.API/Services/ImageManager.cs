using LeadBoard.Shared;
using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Dtos.Settings;
using LeadBoard.Shared.Dtos.Settings.Images;

namespace LeadBoardNet.API.Services;

public class ImageManager : IImageManager
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ImageManager> _logger;

    public ImageManager(ICloudinaryService cloudinaryService, ILogger<ImageManager> logger)
    {
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    public async Task<ImageDetailsSummaryResponse> UploadSingleAsync(IFormFile file)
    {
        var result = await _cloudinaryService.UploadAsync(file);
        if (!result.IsSuccess)
        {
            _logger.LogError("Error al subir imagen a Cloudinary: {Error}", result.Error);
            throw new ImageUploadException(result.Error ?? "Error desconocido", result.StatusCode);
        }

        return new ImageDetailsSummaryResponse(result.Value!.Url, result.Value.PublicId);
    }

    public async Task<List<ImageDetailsSummaryResponse>> UploadMultipleAsync(IEnumerable<IFormFile> files)
    {
        var uploaded = new List<ImageDetailsSummaryResponse>();
        try
        {
            foreach (var file in files)
            {
                var res = await UploadSingleAsync(file);
                uploaded.Add(res);
            }

            return uploaded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo la subida múltiple. Iniciando rollback de {Count} imágenes.", uploaded.Count);
            await RollbackAsync(null, uploaded.Select(x => x.PublicId));
            throw;
        }
    }

    public async Task RollbackAsync(string? publicId, IEnumerable<string>? galleryIds = null)
    {
        var tasks = new List<Task>();

        if (!string.IsNullOrEmpty(publicId))
        {
            tasks.Add(TryDelete(publicId));
        }

        if (galleryIds != null)
        {
            foreach (var id in galleryIds)
            {
                tasks.Add(TryDelete(id));
            }
        }

        // Ejecutamos las eliminaciones en paralelo para mayor velocidad
        await Task.WhenAll(tasks);
    }

    public async Task DeleteAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
        {
            _logger.LogWarning("Se intentó llamar a DeleteAsync con un PublicId nulo o vacío.");
            return;
        }
        
        await TryDelete(publicId);
    }

    private async Task TryDelete(string publicId)
    {
        try
        {
            var result = await _cloudinaryService.DeleteAsync(publicId);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Cloudinary no pudo eliminar el ID: {Id}. Motivo: {Error}", publicId, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar {Id} en Cloudinary", publicId);
        }
    }
}