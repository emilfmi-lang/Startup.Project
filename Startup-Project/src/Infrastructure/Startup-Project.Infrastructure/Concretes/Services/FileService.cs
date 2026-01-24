using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class FileService(IWebHostEnvironment env) : IFileService
{
    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        string wwwrootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
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

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        string wwwrootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        
        string relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        string fullPath = Path.Combine(wwwrootPath, relativePath);

        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath));
        }
    }
}
