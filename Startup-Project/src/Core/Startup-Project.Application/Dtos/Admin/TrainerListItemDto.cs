namespace Startup_Project.Application.Dtos.Admin;

/// <summary>
/// Trainers siyah?s? DTO
/// </summary>
public class TrainerListItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public string Specialization { get; set; } = null!;
    public int Experience { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public int SessionCount { get; set; }
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
