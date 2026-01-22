using LeadBoard.Shared.Dtos.Response;

namespace LeadBoardNet.API.Services;

public interface ICloudinaryService
{
    Task<CloudinaryResponseDto> UploadAsync(IFormFile file);
    Task DeleteAsync(String publicId);
}