namespace Startup_Project.Application.Dtos.GymDtos.GymDetail;

public class GymImageDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = null!;
    public string? Alt { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
}
