
namespace Startup_Project.Application.Dtos.GymDtos.GymReview;

public class ReviewGetDto
{
    public Guid Id { get; set; }
    public string Comment { get; set; } = null!;
    public int StarCount { get; set; }
    public string UserFullName { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string? UserProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedReviewsDto
{
    public List<ReviewGetDto> Reviews { get; set; } = [];
    public int TotalCount { get; set; }
    public double AverageRating { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
