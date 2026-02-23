using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainerPromoCodeService
{
    Task<ResponseModel<Guid>> CreateAsync(TrainerCreatePromoCodeDto dto);
    Task<ResponseModel<List<PromoCodeListDto>>> GetMyPromoCodesAsync(int page = 1, int pageSize = 20);
    Task<ResponseModel<PromoCodeDetailDto>> GetByIdAsync(Guid id);
    Task<ResponseModel<bool>> UpdateAsync(Guid id, UpdatePromoCodeDto dto);
    Task<ResponseModel<bool>> DeactivateAsync(Guid id);
    Task<ResponseModel<bool>> ActivateAsync(Guid id);
    Task<ResponseModel<bool>> DeleteAsync(Guid id);
}
