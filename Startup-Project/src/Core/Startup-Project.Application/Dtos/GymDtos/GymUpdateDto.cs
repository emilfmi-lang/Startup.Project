using Microsoft.AspNetCore.Http;
using Startup_Project.Application.Attributes;

namespace Startup_Project.Application.Dtos.GymDtos;

public class GymUpdateDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    [FileLength(2 * 1024 * 1024)]
    [FileType(new string[] { ".jpg", ".jpeg", ".png" })]
    public IFormFile? NewImage { get; set; }
}
