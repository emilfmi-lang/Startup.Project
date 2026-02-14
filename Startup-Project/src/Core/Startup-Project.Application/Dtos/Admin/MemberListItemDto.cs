namespace Startup_Project.Application.Dtos.Admin;

/// <summary>
/// Admin üçün member siyah?s? döndür?n DTO
/// </summary>
public class MemberListItemDto
{
    /// <summary>
    /// Üzv ID-si
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Tam ad
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Profil ??kili URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Abun?lik planu
    /// </summary>
    public string? Plan { get; set; }

    /// <summary>
    /// Qeydiyyat tarixi
    /// </summary>
    public DateTime JoinDate { get; set; }

    /// <summary>
    /// Abun?lik bitim tarixi
    /// </summary>
    public DateTime? ExpiresDate { get; set; }

    /// <summary>
    /// Check-in say?
    /// </summary>
    public int CheckIns { get; set; }

    /// <summary>
    /// Üzvün v?ziyy?ti (active, inactive, suspended)
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Trainer mü?
    /// </summary>
    public bool IsTrainer { get; set; }

    /// <summary>
    /// Son check-in tarixi
    /// </summary>
    public DateTime? LastCheckIn { get; set; }
}
