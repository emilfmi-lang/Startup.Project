using Microsoft.AspNetCore.Http;
using Startup_Project.Application.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Startup_Project.Application.Dtos.GymDtos;

public class GymCreateDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;

    [Required]
    [FileLength(2 * 1024 * 1024)] 
    [FileType(new string[] { ".jpg", ".jpeg", ".png" })]
    public IFormFile Image { get; set; } = null!;
}
