
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Startup_Project.Application.Attributes;

public class FileLengthAttribute: ValidationAttribute
{
    public readonly long _maxLength;
    public FileLengthAttribute(long maxLength)
    {
        _maxLength = maxLength;
    }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IFormFile file)
            {
                return new ValidationResult("Invalid file.");
            }
            if (file.Length > _maxLength)
            {
                return new ValidationResult($"File size should not exceed {_maxLength} bytes.");
            }
            return ValidationResult.Success;
        }
    }
