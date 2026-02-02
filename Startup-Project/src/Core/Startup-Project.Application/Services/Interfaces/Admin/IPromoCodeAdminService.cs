using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Admin;

public interface IPromoCodeAdminService
{
    Task<ResponseModel<Guid>> CreateAsync(CreatePromoCodeDto dto);
    Task<ResponseModel<List<PromoCodeListDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<ResponseModel<PromoCodeDetailDto>> GetByIdAsync(Guid id);
    Task<ResponseModel<PromoCodeDetailDto>> GetByCodeAsync(string code);
    Task<ResponseModel<bool>> UpdateAsync(Guid id, UpdatePromoCodeDto dto);
    Task<ResponseModel<bool>> DeactivateAsync(Guid id);
    Task<ResponseModel<bool>> ActivateAsync(Guid id);
    Task<ResponseModel<bool>> DeleteAsync(Guid id);
    Task<ResponseModel<List<PromoCodeUsageDto>>> GetUsageHistoryAsync(Guid promoCodeId, int page = 1, int pageSize = 20);
}
