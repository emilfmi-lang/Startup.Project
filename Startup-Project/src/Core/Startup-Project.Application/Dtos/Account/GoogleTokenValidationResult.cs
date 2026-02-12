namespace Startup_Project.Application.Dtos.Account;

/// <summary>
/// Google OAuth2 token do?rulama cavab?
/// </summary>
public class GoogleTokenValidationResult
{
    /// <summary>
    /// Google istifad?çi ID-si
    /// </summary>
    public string GoogleId { get; set; } = null!;

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
    public string? PictureUrl { get; set; }

    /// <summary>
    /// Email t?sdiql?nibmi
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Token düzgündürs? true
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// X?ta mesaj?
    /// </summary>
    public string? ErrorMessage { get; set; }
}
