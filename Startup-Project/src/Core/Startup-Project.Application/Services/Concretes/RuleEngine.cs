using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Concretes;

public class RuleEngine : IRuleEngine
{
    private readonly Dictionary<RuleType, IRuleHandler> _handlers;

    public RuleEngine(IEnumerable<IRuleHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.RuleType, h => h);
    }

    public async Task<bool> EvaluateAsync(Campaign campaign, RuleEvaluationContext context)
    {
        if (campaign.Rules == null || campaign.Rules.Count == 0)
            return true;

        foreach (var rule in campaign.Rules)
        {
            var result = await EvaluateSingleRuleAsync(rule, context);

            if (rule.IsRequired && !result)
                return false;
        }

        return true;
    }

    public async Task<bool> EvaluateSingleRuleAsync(CampaignRule rule, RuleEvaluationContext context)
    {
        if (!_handlers.TryGetValue(rule.RuleType, out var handler))
            return false;

        return await handler.EvaluateAsync(rule, context);
    }
}
