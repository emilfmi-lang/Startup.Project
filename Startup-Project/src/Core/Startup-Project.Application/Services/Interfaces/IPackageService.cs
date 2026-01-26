using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IPackageService
{
    Task<ResponseModel<bool>> CreateAsync(CreatePackageRequestDto dto);
    Task<ResponseModel<bool>> UpdateAsync(string id, UpdatePackageRequestDto dto);
    Task<ResponseModel<bool>> DeleteAsync(string id);
    Task<ResponseModel<List<PackageResponseDto>>> GetAllAsync();
    Task<ResponseModel<PackageResponseDto>> GetByIdAsync(string id);
}
