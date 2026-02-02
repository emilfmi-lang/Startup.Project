using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IPromoCodeService
{
    Task<ResponseModel<PromoCodeResultDto>> ValidateAsync(string code, Guid packageId);
    Task<ResponseModel<PromoCodeResultDto>> ApplyAsync(ApplyPromoCodeDto dto);
}
