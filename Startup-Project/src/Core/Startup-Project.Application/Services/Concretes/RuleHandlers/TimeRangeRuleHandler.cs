using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Concretes.RuleHandlers;

public class TimeRangeRuleHandler : IRuleHandler
{
    public RuleType RuleType => RuleType.TimeRange;

    public Task<bool> EvaluateAsync(CampaignRule rule, RuleEvaluationContext context)
    {
        var currentTime = context.CurrentDateTime.TimeOfDay;

        var result = rule.Operator switch
        {
            Operator.In => EvaluateInRange(rule.Value, currentTime),
            Operator.Equals => EvaluateEquals(rule.Value, currentTime),
            _ => false
        };

        return Task.FromResult(result);
    }

    private static bool EvaluateInRange(string value, TimeSpan currentTime)
    {
        var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return false;

        if (TimeSpan.TryParse(parts[0].Trim(), out var startTime) &&
            TimeSpan.TryParse(parts[1].Trim(), out var endTime))
        {
            if (startTime <= endTime)
            {
                return currentTime >= startTime && currentTime <= endTime;
            }
            else
            {
                return currentTime >= startTime || currentTime <= endTime;
            }
        }

        return false;
    }

    private static bool EvaluateEquals(string value, TimeSpan currentTime)
    {
        if (TimeSpan.TryParse(value, out var targetTime))
        {
            return currentTime.Hours == targetTime.Hours;
        }
        return false;
    }
}
