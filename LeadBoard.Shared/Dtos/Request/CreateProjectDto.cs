using System.ComponentModel.DataAnnotations;

namespace LeadBoard.Shared.Dtos.Request;

public class CreateProjectDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 150 caracteres")]
    public string Title { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 5000 caracteres")]
    public string Description { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [RegularExpression("Residencial|Comercial|Remodelación|Industrial|Institucional",
        ErrorMessage = "Categoría no válida")]
    public string Category { get; set; }

    [Required(ErrorMessage = "La ubicación es obligatoria")]
    [StringLength(100, ErrorMessage = "La ubicación no puede superar los 100 caracteres")]
    public string Location { get; set; }

    [Required(ErrorMessage = "El año del proyecto es obligatorio")]
    [RegularExpression("^(19|20)\\d{2}$", ErrorMessage = "El año debe ser válido (ej: 2023)")]
    public string ProjectYear { get; set; }

    [StringLength(100, ErrorMessage = "El nombre del cliente no puede superar los 100 caracteres")]
    public string ClientName { get; set; }

    [Required(ErrorMessage = "Debe haber al menos un tag")]
    [MinLength(1, ErrorMessage = "Debe haber al menos un tag")]
    public List<TagDto> Tags { get; set; }
}

public class TagDto
{
    [Required(ErrorMessage = "El tag no puede estar vacío")]
    [StringLength(30, ErrorMessage = "El tag no puede superar los 30 caracteres")]
    public string Value { get; set; }
}