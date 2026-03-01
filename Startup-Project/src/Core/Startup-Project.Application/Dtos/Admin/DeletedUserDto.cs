namespace Startup_Project.Application.Dtos.Admin;

public class DeletedUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = [];
    public DateTime? DeletedAt { get; set; }
    public string? DeletionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
