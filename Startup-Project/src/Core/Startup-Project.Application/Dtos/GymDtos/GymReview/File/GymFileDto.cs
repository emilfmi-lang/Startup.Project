
using Microsoft.AspNetCore.Http;
using Startup_Project.Application.Attributes;

namespace Startup_Project.Application.Dtos.GymDtos.GymReview.File;

public class GymFileDto
{
    [FileLength(2 * 1024 * 1024)]
    [FileType([".jpg", ".jpeg", ".png"])]
    public IFormFile? Image { get; set; }
}
