using Microsoft.AspNetCore.Http;

namespace Startup_Project.Application.Services.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName);
}
