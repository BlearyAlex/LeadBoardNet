using FluentValidation;
using LeadBoard.Shared.Dtos.Settings.Projects;

namespace LeadBoardNet.API.Validations;

public class ProjectRequestValidator : AbstractValidator<ProjectRequest>
{
    public ProjectRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(3, 150);

        RuleFor(x => x.Description)
            .NotEmpty()
            .Length(20, 5000);

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(BeValidCategory)
            .WithMessage("Categoría no válida");

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ProjectYear)
            .NotEmpty()
            .Matches("^(19|20)\\d{2}$");

        RuleFor(x => x.ClientName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Tags)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Tags)
            .Must(BeValidTag)
            .NotEmpty()
            .WithMessage("Tag inválido");
    }
    private bool BeValidCategory(string category) =>
        new[] { "Residencial", "Comercial", "Remodelación", "Industrial", "Institucional" }
            .Contains(category);
    
    private bool BeValidTag(TagResponse tag)
    {
        // Aquí tu lógica de validación para cada tag
        return !string.IsNullOrEmpty(tag.Value) && tag.Value.Length <= 50;
    }
}