

namespace Startup_Project.Application.Dtos.TrainerReview;

public class TrainerReviewDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public int StarCount { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
