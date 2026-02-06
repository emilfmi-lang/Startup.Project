namespace Startup_Project.Application.Dtos.Packages;

public class PackageDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }
    public bool IsPopular { get; set; }
    public List<string> Features { get; set; } = [];
    public string DurationLabel { get; set; } = null!;
    public string PriceLabel { get; set; } = null!;
}
