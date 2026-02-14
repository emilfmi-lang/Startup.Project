namespace Startup_Project.Application.Dtos.Admin;

/// <summary>
/// Admin üçün member detail DTO
/// </summary>
public class MemberDetailDto
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
    /// Telefon nömr?si
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Profil ??kili URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Bio
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gym m?lumatlar?
    /// </summary>
    public GymInfoDto? Gym { get; set; }

    /// <summary>
    /// Abun?lik m?lumatlar?
    /// </summary>
    public SubscriptionInfoDto? Subscription { get; set; }

    /// <summary>
    /// Statistikalar
    /// </summary>
    public MemberStatsDto? Stats { get; set; }

    /// <summary>
    /// Trainer profili (?g?r varsa)
    /// </summary>
    public TrainerInfoDto? Trainer { get; set; }

    /// <summary>
    /// Rollar
    /// </summary>
    public List<string> Roles { get; set; } = [];
}

public class GymInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class SubscriptionInfoDto
{
    public Guid Id { get; set; }
    public string Plan { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? ExpiresDate { get; set; }
    public bool IsActive { get; set; }
    public int RemainingDays { get; set; }
}

public class MemberStatsDto
{
    public int TotalCheckIns { get; set; }
    public DateTime? LastCheckIn { get; set; }
    public DateTime JoinDate { get; set; }
    public int TotalSessions { get; set; }
    public double AverageCheckInsPerMonth { get; set; }
}

public class TrainerInfoDto
{
    public Guid Id { get; set; }
    public string Specialization { get; set; } = null!;
    public bool IsVerified { get; set; }
}
