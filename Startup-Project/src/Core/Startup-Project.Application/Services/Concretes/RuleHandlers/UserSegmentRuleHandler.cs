using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Concretes.RuleHandlers;

public class UserSegmentRuleHandler : IRuleHandler
{
    public RuleType RuleType => RuleType.UserSegment;

    public Task<bool> EvaluateAsync(CampaignRule rule, RuleEvaluationContext context)
    {
        var result = rule.Operator switch
        {
            Operator.Equals => EvaluateEquals(rule.Value, context),
            Operator.In => EvaluateIn(rule.Value, context),
            _ => false
        };

        return Task.FromResult(result);
    }

    private static bool EvaluateEquals(string value, RuleEvaluationContext context)
    {
        return string.Equals(context.UserSegment, value, StringComparison.OrdinalIgnoreCase);
    }

    private static bool EvaluateIn(string value, RuleEvaluationContext context)
    {
        if (string.IsNullOrEmpty(context.UserSegment))
            return false;

        var segments = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (string.Equals(context.UserSegment, segment.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
