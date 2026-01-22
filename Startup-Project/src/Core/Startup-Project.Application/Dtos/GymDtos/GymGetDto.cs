

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Startup_Project.Application.Dtos.GymDtos;

public class GymGetDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string OwnerFullName { get; set; } = null!;
    public string? ImagePath { get; set; }
}
