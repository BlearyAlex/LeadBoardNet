using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeadBoard.Shared.Entities;

public class ProjectImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Url { get; set; }
    public string PublicId { get; set; }
    
    public long ProjectId { get; set; }
    public Project Project { get; set; }
}