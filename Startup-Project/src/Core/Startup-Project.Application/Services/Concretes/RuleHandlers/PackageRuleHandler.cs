using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Services.Concretes.RuleHandlers;

public class PackageRuleHandler : IRuleHandler
{
    public RuleType RuleType => RuleType.Package;

    public Task<bool> EvaluateAsync(CampaignRule rule, RuleEvaluationContext context)
    {
        var result = rule.Operator switch
        {
            Operator.Equals => EvaluateEquals(rule.Value, context.PackageId),
            Operator.In => EvaluateIn(rule.Value, context.PackageId),
            _ => false
        };

        return Task.FromResult(result);
    }

    private static bool EvaluateEquals(string value, Guid packageId)
    {
        if (Guid.TryParse(value, out var targetPackageId))
        {
            return packageId == targetPackageId;
        }
        return false;
    }

    private static bool EvaluateIn(string value, Guid packageId)
    {
        var packageIds = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var id in packageIds)
        {
            if (Guid.TryParse(id.Trim(), out var targetPackageId) && packageId == targetPackageId)
            {
                return true;
            }
        }
        return false;
    }
}
