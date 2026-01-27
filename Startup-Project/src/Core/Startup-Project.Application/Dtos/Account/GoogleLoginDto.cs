using System.ComponentModel.DataAnnotations;

namespace Startup_Project.Application.Dtos.Account;

public class GoogleLoginDto
{
    [Required(ErrorMessage = "Google ID token teleb olunur.")]
    public string IdToken { get; set; } = null!;
}
