namespace LeadBoard.Shared.Dtos.Response;

public record CloudinaryResponseDto
{
    public string Url { get; init; } = string.Empty;
    public string PublicId { get; init; } = string.Empty;
}