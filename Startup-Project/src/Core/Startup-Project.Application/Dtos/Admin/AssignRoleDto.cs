using System.ComponentModel.DataAnnotations;

namespace Startup_Project.Application.Dtos.Admin;

public class AssignRoleDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string RoleName { get; set; } = null!;
}
