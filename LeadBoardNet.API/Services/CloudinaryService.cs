using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Dtos.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LeadBoardNet.API.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly List<string> _allowedTypes = new()
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"
    };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public CloudinaryService(IOptions<CloudinarySettings> configuration)
    {
        var account = new Account(
            configuration.Value.CloudName,
            configuration.Value.ApiKey,
            configuration.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<CloudinaryResponseDto> UploadAsync(IFormFile file)
    {
        ValidateFile(file);

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, stream),
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Error al subir a Cloudinary: {uploadResult.Error.Message}");
        }

        return new CloudinaryResponseDto
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }

    public async Task DeleteAsync(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Result != "ok")
        {
            throw new Exception($"No se pudo eliminar el archivo: {result.Error?.Message}");
        }
    }
    
    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("El archivo está vacío");

        if (file.Length > MaxFileSize)
            throw new ArgumentException($"El archivo excede el tamaño máximo de {MaxFileSize / 1024 / 1024}MB");

        if (!_allowedTypes.Contains(file.ContentType))
            throw new ArgumentException("Tipo de archivo no permitido");
    }
}