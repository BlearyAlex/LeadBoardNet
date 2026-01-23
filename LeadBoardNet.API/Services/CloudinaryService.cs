using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Dtos.Settings;
using LeadBoard.Shared.Wrappers;
using Microsoft.Extensions.Options;

namespace LeadBoardNet.API.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    private readonly List<string> _allowedTypes = new()
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public CloudinaryService(
        IOptions<CloudinarySettings> configuration,
        ILogger<CloudinaryService> logger)
    {
        var account = new Account(
            configuration.Value.CloudName,
            configuration.Value.ApiKey,
            configuration.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
        _logger = logger;
    }

    public async Task<Result<CloudinaryResponseDto>> UploadAsync(IFormFile file)
    {
        var validationResult = ValidateFile(file);
        if (!validationResult.IsSuccess)
            return validationResult;

        try
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Error de Cloudinary al subir {fileName}: {Error}",
                    file.FileName, uploadResult.Error.Message);

                return Result<CloudinaryResponseDto>.Failure(
                    $"Error al subir el archivo: {uploadResult.Error.Message}",
                    HttpStatusCode.BadRequest
                );
            }

            var response = new CloudinaryResponseDto
            {
                Url = uploadResult.SecureUri.AbsoluteUri,
                PublicId = uploadResult.PublicId
            };

            return Result<CloudinaryResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            // Este error técnico sí puede ir al middleware, pero lo manejamos aquí
            _logger.LogError(ex, "Excepción al subir archivo a Cloudinary: {FileName}", file.FileName);
            return Result<CloudinaryResponseDto>.Failure("Error al procesar la imagen. Intente nuevamente.",
                HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return Result<bool>.BadRequest("El publicId es requerido.");

        try
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result != "ok")
            {
                _logger.LogWarning("No se pudo eliminar imagen de Cloudinary: {PublicId}, Razón: {Error}",
                    publicId, result.Error?.Message);

                return Result<bool>.Failure(
                    "No se pudo eliminar la imagen",
                    HttpStatusCode.BadRequest
                );
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al eliminar de Cloudinary: {PublicId}", publicId);

            return Result<bool>.Failure(
                "Error al eliminar la imagen",
                HttpStatusCode.InternalServerError
            );
        }
    }

    private Result<CloudinaryResponseDto> ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Result<CloudinaryResponseDto>.BadRequest("El archivo está vacío o no fue proporcionado");

        if (file.Length > MaxFileSize)
            return Result<CloudinaryResponseDto>.BadRequest(
                $"El archivo excede el tamaño máximo de {MaxFileSize / 1024 / 1024}MB"
            );

        if (!_allowedTypes.Contains(file.ContentType.ToLower()))
            return Result<CloudinaryResponseDto>.BadRequest(
                $"Tipo de archivo no permitido. Tipos aceptados: {string.Join(", ", _allowedTypes)}"
            );

        return Result<CloudinaryResponseDto>.Success(null!); // Solo para indicar validación OK
    }
}
