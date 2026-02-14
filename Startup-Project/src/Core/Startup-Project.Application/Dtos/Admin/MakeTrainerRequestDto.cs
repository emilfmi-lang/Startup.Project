namespace Startup_Project.Application.Dtos.Admin;

/// <summary>
/// Member-i trainr? çevirm? üçün request DTO
/// </summary>
public class MakeTrainerRequestDto
{
    /// <summary>
    /// Trainer ixtisasla?mas? (Strength Training, Cardio, Yoga v?s)
    /// </summary>
    public string Specialization { get; set; } = null!;

    /// <summary>
    /// ?? t?crüb?si (il)
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Trainer bio
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Sertifikat URL-i
    /// </summary>
    public string? CertificateUrl { get; set; }

    /// <summary>
    /// Sessiya qiym?ti
    /// </summary>
    public decimal SessionPrice { get; set; }
}
