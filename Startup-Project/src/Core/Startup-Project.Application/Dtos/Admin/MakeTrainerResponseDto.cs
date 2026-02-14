namespace Startup_Project.Application.Dtos.Admin;

/// <summary>
/// Member-i trainer-? çevirm? response
/// Yeni JWT token il? döndürülür ki, trainer role-? giri? ed? bilsin
/// </summary>
public class MakeTrainerResponseDto
{
    /// <summary>
    /// Yeni trainer m?lumatlar?
    /// </summary>
    public TrainerListItemDto Trainer { get; set; } = null!;

    /// <summary>
    /// Yeni JWT token (trainer role il?)
    /// Frontend bu token-i localStorage-? saxlamal?d?r
    /// </summary>
    public string NewToken { get; set; } = null!;

    /// <summary>
    /// Success mesaj?
    /// </summary>
    public string Message { get; set; } = "Member successfully promoted to trainer";
}
