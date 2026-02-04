using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Campaign;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Application.Services.Concretes;

public class CampaignService(
    IGenericRepository<Campaign> campaignRepository,
    IMapper mapper) : ICampaignService
{
    public async Task<ResponseModel<List<CampaignListDto>>> GetActiveCampaignsAsync()
    {
        var now = DateTime.UtcNow;

        var campaigns = await campaignRepository.Table
            .AsNoTracking()
            .Where(c => c.IsActive &&
                        !c.IsDeleted &&
                        c.StartDate <= now &&
                        c.EndDate >= now)
            .OrderBy(c => c.Priority)
            .ToListAsync();

        var result = mapper.Map<List<CampaignListDto>>(campaigns);
        return ResponseModel<List<CampaignListDto>>.SuccessResponse(result);
    }

    public async Task<ResponseModel<CampaignDetailDto>> GetCampaignByIdAsync(Guid id)
    {
        var campaign = await campaignRepository.Table
            .AsNoTracking()
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(id);

        var result = mapper.Map<CampaignDetailDto>(campaign);
        return ResponseModel<CampaignDetailDto>.SuccessResponse(result);
    }
}
