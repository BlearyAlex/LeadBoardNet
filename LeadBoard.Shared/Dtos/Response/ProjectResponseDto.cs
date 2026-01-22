namespace LeadBoard.Shared.Dtos.Response;

public class ProjectResponseDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string MainImageUrl { get; set; }
    public string MainImagePublicId { get; set; }
    public string Category { get; set; }      // Residencial, Comercial, etc
    public string Location { get; set; }      // Ciudad o país
    public string ProjectYear { get; set; }
    public string ClientName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; }
    public ICollection<string> Tags { get; set; } = new List<string>();
    public ICollection<ProjectImageResponseDto> Gallery { get; set; } = new List<ProjectImageResponseDto>();
}