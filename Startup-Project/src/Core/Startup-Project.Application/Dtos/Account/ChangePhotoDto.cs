using Microsoft.AspNetCore.Http;

namespace Startup_Project.Application.Dtos.Account;

public class ChangePhotoDto
{
    public IFormFile Photo { get; set; } = null!;

}
