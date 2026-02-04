using Startup_Project.Application.Models.Common;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Concretes.RuleHandlers;

public class MinimumOrderAmountRuleHandler : IRuleHandler
{
    public RuleType RuleType => RuleType.MinimumOrderAmount;

    public Task<bool> EvaluateAsync(CampaignRule rule, RuleEvaluationContext context)
    {
        if (!decimal.TryParse(rule.Value, out var targetAmount))
            return Task.FromResult(false);

        var result = rule.Operator switch
        {
            Operator.Equals => context.OrderAmount == targetAmount,
            Operator.GreaterThan => context.OrderAmount > targetAmount,
            Operator.GreaterThanOrEqual => context.OrderAmount >= targetAmount,
            Operator.LessThan => context.OrderAmount < targetAmount,
            _ => false
        };

        return Task.FromResult(result);
    }
}
