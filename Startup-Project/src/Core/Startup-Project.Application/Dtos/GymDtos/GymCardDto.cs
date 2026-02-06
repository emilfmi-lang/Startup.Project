namespace Startup_Project.Application.Dtos.GymDtos;

public class GymCardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsOpen { get; set; }
    public bool IsFavorite { get; set; }
    public List<string> Tags { get; set; } = [];
    public decimal? MinPrice { get; set; }
    public string? PriceLabel { get; set; }
}
