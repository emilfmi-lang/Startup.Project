namespace Startup_Project.Application.Dtos.Admin;

public class UserListDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = [];
    public bool EmailConfirmed { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? CreatedAt { get; set; }
}
