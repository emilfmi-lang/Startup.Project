namespace Startup_Project.Application.Dtos.Favorites;

public class FavoriteTrainerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public string? GymName { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string RatingLabel { get; set; } = null!;
    public decimal SessionPrice { get; set; }
    public string SessionPriceLabel { get; set; } = null!;
    public int CompletedSessions { get; set; }
    public string SessionsLabel { get; set; } = null!;
    public bool IsFavorite { get; set; } = true;
}
