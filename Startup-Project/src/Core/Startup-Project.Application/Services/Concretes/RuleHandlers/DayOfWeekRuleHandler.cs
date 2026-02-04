using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Concretes.RuleHandlers;

public class DayOfWeekRuleHandler : IRuleHandler
{
    public RuleType RuleType => RuleType.DayOfWeek;

    public Task<bool> EvaluateAsync(CampaignRule rule, RuleEvaluationContext context)
    {
        var currentDayOfWeek = (int)context.CurrentDateTime.DayOfWeek;

        var result = rule.Operator switch
        {
            Operator.Equals => EvaluateEquals(rule.Value, currentDayOfWeek),
            Operator.In => EvaluateIn(rule.Value, currentDayOfWeek),
            _ => false
        };

        return Task.FromResult(result);
    }

    private static bool EvaluateEquals(string value, int currentDayOfWeek)
    {
        if (int.TryParse(value, out var targetDay))
        {
            return currentDayOfWeek == targetDay;
        }
        return false;
    }

    private static bool EvaluateIn(string value, int currentDayOfWeek)
    {
        var days = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var day in days)
        {
            if (int.TryParse(day.Trim(), out var targetDay) && currentDayOfWeek == targetDay)
            {
                return true;
            }
        }
        return false;
    }
}
