using Startup_Project.Application.Dtos.Campaign;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Admin;

public interface ICampaignAdminService
{
    Task<ResponseModel<Guid>> CreateAsync(CreateCampaignDto dto);
    Task<ResponseModel<List<CampaignListDto>>> GetAllAsync(int page, int pageSize);
    Task<ResponseModel<CampaignDetailDto>> GetByIdAsync(Guid id);
    Task<ResponseModel<bool>> UpdateAsync(Guid id, UpdateCampaignDto dto);
    Task<ResponseModel<bool>> DeleteAsync(Guid id);
    Task<ResponseModel<bool>> ActivateAsync(Guid id);
    Task<ResponseModel<bool>> DeactivateAsync(Guid id);
    Task<ResponseModel<Guid>> AddRuleAsync(Guid campaignId, CreateCampaignRuleDto dto);
    Task<ResponseModel<bool>> RemoveRuleAsync(Guid campaignId, Guid ruleId);
    Task<ResponseModel<List<CampaignUsageDto>>> GetUsageHistoryAsync(Guid campaignId, int page, int pageSize);
}
