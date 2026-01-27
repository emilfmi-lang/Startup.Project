
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Startup_Project.Application.Attributes;

public class FileTypeAttribute : ValidationAttribute
{
    private readonly string[] _allowedExtensions;

    public FileTypeAttribute(string[] allowedExtensions)
    {
        _allowedExtensions = allowedExtensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
            return ValidationResult.Success; 

        var ext = Path.GetExtension(file.FileName).ToLower();

        if (!_allowedExtensions.Contains(ext))
        {
            var allowed = string.Join(", ", _allowedExtensions);
            return new ValidationResult($"Invalid file type. Allowed: {allowed}");
        }

        return ValidationResult.Success;
    }
}
