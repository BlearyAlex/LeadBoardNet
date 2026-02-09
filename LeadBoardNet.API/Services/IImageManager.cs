using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Dtos.Settings;
using LeadBoard.Shared.Dtos.Settings.Images;

namespace LeadBoardNet.API.Services;

public interface IImageManager
{
    Task<ImageDetailsSummaryResponse> UploadSingleAsync(IFormFile file);
    Task<List<ImageDetailsSummaryResponse>> UploadMultipleAsync(IEnumerable<IFormFile> files);
    Task RollbackAsync(string? publicId, IEnumerable<string>? galleryIds = null);
    Task DeleteAsync(string publicId);
}