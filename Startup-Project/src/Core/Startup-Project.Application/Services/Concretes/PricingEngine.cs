using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Pricing;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class PricingEngine(
    IGenericRepository<Campaign> campaignRepository,
    IGenericRepository<Package> packageRepository,
    IGenericRepository<CampaignUsage> campaignUsageRepository,
    IGenericRepository<Subscription> subscriptionRepository,
    IPromoCodeService promoCodeService,
    IRuleEngine ruleEngine,
    IHttpContextAccessor httpContextAccessor) : IPricingEngine
{
    public async Task<ResponseModel<PricingResultDto>> CalculateAsync(Guid packageId, string? promoCode)
    {
        var userId = GetCurrentUserId();
        var result = await CalculatePriceInternalAsync(packageId, promoCode, userId, persistUsage: true);
        return ResponseModel<PricingResultDto>.SuccessResponse(result);
    }

    public async Task<ResponseModel<PricingResultDto>> PreviewAsync(Guid packageId)
    {
        Guid? userId = null;
        try
        {
            userId = GetCurrentUserId();
        }
        catch
        {
            // Anonymous user - no userId
        }

        var result = await CalculatePriceInternalAsync(packageId, null, userId, persistUsage: false);
        return ResponseModel<PricingResultDto>.SuccessResponse(result);
    }

    private async Task<PricingResultDto> CalculatePriceInternalAsync(
        Guid packageId,
        string? promoCode,
        Guid? userId,
        bool persistUsage)
    {
        // 1. Package-i yükl?
        var package = await packageRepository.GetByIdAsync(packageId);
        if (package == null)
            throw NotFoundException.Package(packageId);

        var originalPrice = package.Price;
        var currentPrice = originalPrice;
        var appliedCampaigns = new List<AppliedDiscountDto>();
        decimal totalCampaignDiscount = 0;

        // 2. RuleEvaluationContext yarat
        var context = await BuildRuleContextAsync(packageId, originalPrice, userId);

        // 3. Aktiv Campaign-lar? tap
        var now = DateTime.UtcNow;
        var activeCampaigns = await campaignRepository.Table
            .Include(c => c.Rules)
            .Where(c => c.IsActive &&
                        !c.IsDeleted &&
                        c.StartDate <= now &&
                        c.EndDate >= now &&
                        (!c.UsageLimit.HasValue || c.UsedCount < c.UsageLimit.Value))
            .OrderBy(c => c.Priority)
            .ToListAsync();

        // 4. Rule-lar? evaluate et v? uy?un olanlar? seç
        var applicableCampaigns = new List<Campaign>();
        foreach (var campaign in activeCampaigns)
        {
            if (!campaign.IsApplicableToOrder(originalPrice))
                continue;

            var isMatch = await ruleEngine.EvaluateAsync(campaign, context);
            if (isMatch)
            {
                applicableCampaigns.Add(campaign);
            }
        }

        // 5. Stackable v? non-stackable kampaniyalar? ay?r
        var nonStackable = applicableCampaigns.Where(c => !c.IsStackable).OrderBy(c => c.Priority).FirstOrDefault();
        var stackable = applicableCampaigns.Where(c => c.IsStackable).OrderBy(c => c.Priority).ToList();

        // 6. Kampaniyalar? t?tbiq et
        var campaignsToApply = new List<Campaign>();

        if (nonStackable != null)
        {
            // Non-stackable varsa, ya onu ya da stackable-lar? seç
            var nonStackableDiscount = nonStackable.CalculateDiscount(currentPrice);
            var stackableDiscount = CalculateStackableDiscount(stackable, currentPrice);

            if (nonStackableDiscount >= stackableDiscount)
            {
                campaignsToApply.Add(nonStackable);
            }
            else
            {
                campaignsToApply.AddRange(stackable);
            }
        }
        else
        {
            campaignsToApply.AddRange(stackable);
        }

        // 7. Seçil?n kampaniyalar? t?tbiq et
        foreach (var campaign in campaignsToApply)
        {
            var discount = campaign.CalculateDiscount(currentPrice);
            if (discount <= 0) continue;

            currentPrice -= discount;
            totalCampaignDiscount += discount;

            appliedCampaigns.Add(new AppliedDiscountDto
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                DiscountType = campaign.DiscountType.ToString(),
                DiscountValue = campaign.DiscountValue,
                DiscountAmount = discount,
                Priority = campaign.Priority
            });

            // Usage-? persist et
            if (persistUsage && userId.HasValue)
            {
                campaign.IncrementUsedCount();
                campaignRepository.Update(campaign);

                var usage = new CampaignUsage(
                    campaign.Id,
                    userId.Value,
                    packageId,
                    originalPrice,
                    discount);
                await campaignUsageRepository.AddAsync(usage);
            }
        }

        // 8. PromoCode t?tbiq et
        AppliedPromoCodeDto? appliedPromoCode = null;
        decimal promoCodeDiscount = 0;

        if (!string.IsNullOrWhiteSpace(promoCode) && userId.HasValue && persistUsage)
        {
            try
            {
                var promoResult = await promoCodeService.ValidateAsync(promoCode, packageId);
                if (promoResult.Data?.IsValid == true)
                {
                    // Promokod endirimi cari qiym?t? t?tbiq olunur
                    var promoDiscountAmount = CalculatePromoDiscount(
                        promoResult.Data.DiscountType,
                        promoResult.Data.DiscountValue,
                        currentPrice);

                                if (promoDiscountAmount > 0)
                                {
                                    currentPrice -= promoDiscountAmount;
                                    promoCodeDiscount = promoDiscountAmount;

                                    appliedPromoCode = new AppliedPromoCodeDto
                                    {
                                        Code = promoCode.ToUpperInvariant(),
                                        DiscountType = promoResult.Data.DiscountType,
                                        DiscountValue = promoResult.Data.DiscountValue,
                                        DiscountAmount = promoDiscountAmount
                                    };

                                    // PromoCode apply et
                                    await promoCodeService.ApplyAsync(new ApplyPromoCodeDto(promoCode, packageId));
                                }
                            }
                        }
                        catch
                        {
                            // PromoCode x?tas? pricing-i dayand?rmamal?d?r
                        }
                    }

                    // 9. Save changes
                    if (persistUsage)
                    {
            await campaignRepository.SaveAsync();
        }

        // 10. Final qiym?ti minimum 0 olmal?d?r
        if (currentPrice < 0) currentPrice = 0;

        var totalDiscount = totalCampaignDiscount + promoCodeDiscount;

        return new PricingResultDto
        {
            OriginalPrice = originalPrice,
            FinalPrice = Math.Round(currentPrice, 2),
            TotalDiscount = Math.Round(totalDiscount, 2),
            CampaignDiscount = Math.Round(totalCampaignDiscount, 2),
            PromoCodeDiscount = Math.Round(promoCodeDiscount, 2),
            AppliedCampaigns = appliedCampaigns,
            AppliedPromoCode = appliedPromoCode,
            Message = BuildResultMessage(originalPrice, currentPrice, appliedCampaigns.Count, appliedPromoCode != null)
        };
    }

    private async Task<RuleEvaluationContext> BuildRuleContextAsync(Guid packageId, decimal orderAmount, Guid? userId)
    {
        var context = new RuleEvaluationContext
        {
            PackageId = packageId,
            OrderAmount = orderAmount,
            CurrentDateTime = DateTime.UtcNow
        };

        if (userId.HasValue)
        {
            context.UserId = userId.Value;

            // User segment-i mü?yy?n et
            var hasSubscription = await subscriptionRepository.AnyAsync(
                s => s.UserId == userId.Value && !s.IsDeleted);

            context.IsNewUser = !hasSubscription;
            context.UserSegment = hasSubscription ? "ExistingUser" : "NewUser";

            // Total purchase count
            context.TotalPurchaseCount = await subscriptionRepository.Table
                .CountAsync(s => s.UserId == userId.Value && !s.IsDeleted);
        }

        return context;
    }

    private static decimal CalculateStackableDiscount(List<Campaign> campaigns, decimal price)
    {
        decimal totalDiscount = 0;
        var currentPrice = price;

        foreach (var campaign in campaigns)
        {
            var discount = campaign.CalculateDiscount(currentPrice);
            totalDiscount += discount;
            currentPrice -= discount;
        }

        return totalDiscount;
    }

    private static decimal CalculatePromoDiscount(string discountType, decimal discountValue, decimal currentPrice)
    {
        if (discountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
        {
            return currentPrice * (discountValue / 100);
        }
        return Math.Min(discountValue, currentPrice);
    }

    private static string BuildResultMessage(decimal original, decimal final, int campaignCount, bool hasPromo)
    {
        var discount = original - final;
        if (discount <= 0)
            return "Heç bir endirim t?tbiq olunmad?.";

        var parts = new List<string>();
        if (campaignCount > 0)
            parts.Add($"{campaignCount} kampaniya");
        if (hasPromo)
            parts.Add("promokod");

        return $"{string.Join(" v? ", parts)} t?tbiq olundu. Ümumi endirim: {discount:F2} AZN";
    }

    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            throw BusinessException.UnauthorizedAccess();
        return userGuid;
    }
}
