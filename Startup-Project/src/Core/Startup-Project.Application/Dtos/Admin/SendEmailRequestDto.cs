namespace Startup_Project.Application.Dtos.Admin;

/// <summary>
/// Admin panelinden email gondermek ucun request DTO
/// </summary>
public class SendEmailRequestDto
{
    /// <summary>
    /// Gonderilecek email unvani
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Email movzusu
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Email metni (HTML destekleyir)
    /// </summary>
    public string Body { get; set; } = null!;
}
