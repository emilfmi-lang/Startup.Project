
namespace Startup_Project.Application.Dtos.GymDtos.GymReview;

public class ReviewGetDto
{
    public Guid Id { get; set; }
    public string Comment { get; set; }
    public int StarCount { get; set; }
    public string UserFullName { get; set; } 
    public DateTime CreatedAt { get; set; }
}
