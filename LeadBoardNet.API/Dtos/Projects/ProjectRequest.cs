namespace LeadBoard.Shared.Dtos.Settings.Projects;

public record ProjectRequest(
    string Title,
    string Description,
    string Category,
    string Location,
    string ProjectYear,
    string ClientName,
    List<TagResponse> Tags);