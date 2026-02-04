using Startup_Project.Application.Models;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Services.Interfaces;

public interface IRuleEngine
{
    Task<bool> EvaluateAsync(Campaign campaign, RuleEvaluationContext context);
    Task<bool> EvaluateSingleRuleAsync(CampaignRule rule, RuleEvaluationContext context);
}
