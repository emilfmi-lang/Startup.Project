using Startup_Project.Application.Models.Common;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Interfaces;

public interface IRuleHandler
{
    RuleType RuleType { get; }
    Task<bool> EvaluateAsync(CampaignRule rule, RuleEvaluationContext context);
}
