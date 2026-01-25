
namespace Startup_Project.Application.Dtos.GymDtos;

public class GymListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } 
    public string Address { get; set; } 
    public double AverageRating { get; set; }
    public bool IsOpen { get; set; }
}
