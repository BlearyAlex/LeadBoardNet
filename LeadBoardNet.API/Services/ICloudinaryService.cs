using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Wrappers;

namespace LeadBoardNet.API.Services;

public interface ICloudinaryService
{
    Task<Result<CloudinaryResponseDto>> UploadAsync(IFormFile file);
    Task<Result<bool>> DeleteAsync(string publicId);
}
