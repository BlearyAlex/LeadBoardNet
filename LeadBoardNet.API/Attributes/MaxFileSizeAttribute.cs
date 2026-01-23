using System.ComponentModel.DataAnnotations;

namespace LeadBoardNet.API.Attributes;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }
    
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                return new ValidationResult(
                    $"El archivo excede el tamaño máximo de {_maxFileSize / 1024 / 1024}MB");
            }
        }
        return ValidationResult.Success;
    }
}