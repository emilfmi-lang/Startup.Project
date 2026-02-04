using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Campaign;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Admin;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Application.Services.Concretes.Admin;

public class CampaignAdminService(
    IGenericRepository<Campaign> campaignRepository,
    IGenericRepository<CampaignRule> ruleRepository,
    IGenericRepository<CampaignUsage> usageRepository,
    IValidator<CreateCampaignDto> createValidator,
    IValidator<UpdateCampaignDto> updateValidator,
    IMapper mapper) : ICampaignAdminService
{
    public async Task<ResponseModel<Guid>> CreateAsync(CreateCampaignDto dto)
    {
        var validationResult = await createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var campaign = new Campaign(
            dto.Name,
            dto.Description,
            dto.DiscountType,
            dto.DiscountValue,
            dto.StartDate,
            dto.EndDate,
            dto.Priority,
            dto.IsStackable,
            dto.TargetType,
            dto.MaxDiscountAmount,
            dto.MinimumOrderAmount,
            dto.UsageLimit);

        await campaignRepository.AddAsync(campaign);

        if (dto.Rules != null && dto.Rules.Count > 0)
        {
            foreach (var ruleDto in dto.Rules)
            {
                var rule = new CampaignRule(
                    campaign.Id,
                    ruleDto.RuleType,
                    ruleDto.Operator,
                    ruleDto.Value,
                    ruleDto.IsRequired,
                    ruleDto.Description);

                campaign.AddRule(rule);
            }
        }

        await campaignRepository.SaveAsync();

        return ResponseModel<Guid>.SuccessResponse(campaign.Id);
    }

    public async Task<ResponseModel<List<CampaignListDto>>> GetAllAsync(int page, int pageSize)
    {
        var campaigns = await campaignRepository.Table
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = mapper.Map<List<CampaignListDto>>(campaigns);
        return ResponseModel<List<CampaignListDto>>.SuccessResponse(result);
    }

    public async Task<ResponseModel<CampaignDetailDto>> GetByIdAsync(Guid id)
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

    public async Task<ResponseModel<bool>> UpdateAsync(Guid id, UpdateCampaignDto dto)
    {
        var validationResult = await updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var campaign = await campaignRepository.Table
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(id);

        campaign.Update(
            dto.Name,
            dto.Description,
            dto.DiscountValue,
            dto.MaxDiscountAmount,
            dto.MinimumOrderAmount,
            dto.Priority,
            dto.IsStackable,
            dto.UsageLimit,
            dto.StartDate,
            dto.EndDate);

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value)
                campaign.Activate();
            else
                campaign.Deactivate();
        }

        campaignRepository.Update(campaign);
        await campaignRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(Guid id)
    {
        var campaign = await campaignRepository.Table
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(id);

        campaign.IsDeleted = true;
        campaignRepository.Update(campaign);
        await campaignRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ActivateAsync(Guid id)
    {
        var campaign = await campaignRepository.Table
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(id);

        campaign.Activate();
        campaignRepository.Update(campaign);
        await campaignRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeactivateAsync(Guid id)
    {
        var campaign = await campaignRepository.Table
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(id);

        campaign.Deactivate();
        campaignRepository.Update(campaign);
        await campaignRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<Guid>> AddRuleAsync(Guid campaignId, CreateCampaignRuleDto dto)
    {
        var campaign = await campaignRepository.Table
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(campaignId);

        var rule = new CampaignRule(
            campaignId,
            dto.RuleType,
            dto.Operator,
            dto.Value,
            dto.IsRequired,
            dto.Description);

        campaign.AddRule(rule);
        await ruleRepository.AddAsync(rule);
        await ruleRepository.SaveAsync();

        return ResponseModel<Guid>.SuccessResponse(rule.Id);
    }

    public async Task<ResponseModel<bool>> RemoveRuleAsync(Guid campaignId, Guid ruleId)
    {
        var campaign = await campaignRepository.Table
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(campaignId);

        var rule = campaign.Rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule == null)
            throw NotFoundException.CampaignRule(ruleId);

        campaign.RemoveRule(ruleId);
        rule.IsDeleted = true;
        ruleRepository.Update(rule);
        await ruleRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<CampaignUsageDto>>> GetUsageHistoryAsync(Guid campaignId, int page, int pageSize)
    {
        var campaign = await campaignRepository.Table
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

        if (campaign == null)
            throw NotFoundException.Campaign(campaignId);

        var usages = await usageRepository.Table
            .AsNoTracking()
            .Where(u => u.CampaignId == campaignId)
            .OrderByDescending(u => u.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = mapper.Map<List<CampaignUsageDto>>(usages);
        return ResponseModel<List<CampaignUsageDto>>.SuccessResponse(result);
    }
}
