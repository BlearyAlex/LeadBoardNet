using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeadBoard.Shared.Entities;

public class Project
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }
    
    public string Description { get; set; }
    public string MainImageUrl { get; set; }
    public string MainImagePublicId { get; set; }
    public string Category { get; set; }      // Residencial, Comercial, etc
    public string Location { get; set; }      // Ciudad o país
    public string ProjectYear { get; set; }
    public string ClientName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ProjectStatus Status { get; set; }
    public ICollection<ProjectTag> Tags { get; set; } = new List<ProjectTag>();
    public ICollection<ProjectImage> Gallery { get; set; } = new List<ProjectImage>();
}

public enum ProjectStatus
{
    COMPLETED,
    UNDER_CONSTRUCTION,
    CONCEPTUAL
}