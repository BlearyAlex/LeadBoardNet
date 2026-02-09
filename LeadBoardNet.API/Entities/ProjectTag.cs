using System.ComponentModel.DataAnnotations;

namespace LeadBoard.Shared.Entities;

public class ProjectTag
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string Value { get; set; }
    
    public long ProjectId { get; set; }
    public Project Project { get; set; }
}