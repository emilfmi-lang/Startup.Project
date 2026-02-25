

namespace Startup_Project.Application.Dtos.TrainerReview;

public class TrainerReviewDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string? UserProfileImageUrl { get; set; }
    public int StarCount { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedTrainerReviewsDto
{
    public List<TrainerReviewDto> Reviews { get; set; } = [];
    public int TotalCount { get; set; }
    public double AverageRating { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
