using LeadBoard.Shared.Dtos.Settings;

public record ProjectResponse(
    string Title,
    string Description,
    string Category,
    string Location,
    string ProjectYear,
    string? ClientName,
    IReadOnlyList<TagDto> Tags,
    string MainImageUrl,
    string MainImagePublicId,
    DateTime CreatedAt,
    string Status,
    IReadOnlyList<ImageDetails> Gallery);

public record TagDto(string Value);

public record ProjectRequest(
    string Title,
    string Description,
    string Category,
    string Location,
    string ProjectYear,
    string ClientName,
    List<TagDto> Tags);
