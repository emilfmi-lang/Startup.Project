
namespace Startup_Project.Application.Dtos.GymDtos;

public class GymListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? ImagePath { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsOpen { get; set; }
    public bool IsFavorite { get; set; }
    public int FavoriteCount { get; set; }
    public List<string> Tags { get; set; } = [];
    public decimal? MinMonthlyPrice { get; set; }
}
