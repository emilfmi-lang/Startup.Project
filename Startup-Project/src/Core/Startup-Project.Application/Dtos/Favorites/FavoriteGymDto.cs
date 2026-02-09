namespace Startup_Project.Application.Dtos.Favorites;

public class FavoriteGymDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public bool IsPremium { get; set; }
    public string StatusLabel { get; set; } = null!;
    public int ReviewCount { get; set; }
    public string RatingLabel { get; set; }
    public double Rating { get; set; }
    public List<string> Amenities { get; set; } = [];
    public decimal MinPrice { get; set; }
    public bool IsOpen { get; set; }
    public decimal MaxPrice { get; set; }
    public string PriceLabel { get; set; } = null!;
    public bool IsFavorite { get; set; } = true;
}
