using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Services;

public class FileService(IWebHostEnvironment _env) : IFileService
{
    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        string wwwrootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string folderPath = Path.Combine(wwwrootPath, "uploads", folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string fullPath = Path.Combine(folderPath, fileName);
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return $"/uploads/{folderName}/{fileName}";
    }
}
