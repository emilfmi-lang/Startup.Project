namespace Startup_Project.Application.Dtos.Packages;

public class PackageFeatureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
