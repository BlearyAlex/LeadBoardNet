using LeadBoard.Shared.Dtos.Response;
using LeadBoard.Shared.Dtos.Settings;

namespace LeadBoardNet.API.Services;

public interface IImageManager
{
    Task<ImageDetailsSummary> UploadSingleAsync(IFormFile file);
    Task<List<ImageDetailsSummary>> UploadMultipleAsync(IEnumerable<IFormFile> files);
    Task RollbackAsync(string? publicId, IEnumerable<string>? galleryIds = null);
    Task DeleteAsync(string publicId);
}