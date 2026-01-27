using System.ComponentModel.DataAnnotations;

namespace Startup_Project.Application.Dtos.Admin;

public class AssignGymOwnerDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid GymId { get; set; }
}
