using System.ComponentModel.DataAnnotations;

namespace LeadBoardNet.API.Attributes;

public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(params string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!_extensions.Contains(extension))
            {
                return new ValidationResult(
                    $"Extensión no permitida. Permitidas: {string.Join(", ", _extensions)}");
            }
        }
        return ValidationResult.Success;
    }
}