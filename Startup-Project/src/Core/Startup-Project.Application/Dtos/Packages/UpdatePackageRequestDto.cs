

namespace Startup_Project.Application.Dtos.Packages;

public class UpdatePackageRequestDto
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }
    public Guid GymId { get; set; }
}
