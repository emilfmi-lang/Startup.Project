namespace Startup_Project.Infrastructure.Security;

/// <summary>
/// Gmail SMTP tenzimleri
/// Google App Password istifade olunmalidir (normal Gmail parolu islemir)
/// Google Account -> Security -> 2-Step Verification -> App passwords
/// </summary>
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpServer { get; init; } = null!;
    public int Port { get; init; }
    public string SenderName { get; init; } = null!;
    public string SenderEmail { get; init; } = null!;

    /// <summary>
    /// Google App Password — normal Gmail parolu deyil!
    /// Google hesabinda 2FA aktiv edilmeli ve App Password yaradilmalidir
    /// </summary>
    public string AppPassword { get; init; } = null!;
}
