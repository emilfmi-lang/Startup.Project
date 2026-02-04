using Startup_Project.Application.Dtos.Campaign;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ICampaignService
{
    Task<ResponseModel<List<CampaignListDto>>> GetActiveCampaignsAsync();
    Task<ResponseModel<CampaignDetailDto>> GetCampaignByIdAsync(Guid id);
}
