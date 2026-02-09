using LeadBoard.Shared.Dtos.Settings.Images;

namespace LeadBoard.Shared.Dtos.Settings.Projects;

public record ProjectResponse(
    string Title,
    string Description,
    string Category,
    string Location,
    string ProjectYear,
    string? ClientName,
    IReadOnlyList<TagResponse> Tags,
    string MainImageUrl,
    string MainImagePublicId,
    DateTime CreatedAt,
    string Status,
    IReadOnlyList<ImageDetailsResponse> Gallery);
    
public record TagResponse(string Value);