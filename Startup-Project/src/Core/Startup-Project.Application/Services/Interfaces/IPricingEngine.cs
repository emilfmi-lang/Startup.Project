using Startup_Project.Application.Dtos.Pricing;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IPricingEngine
{
    Task<ResponseModel<PricingResultDto>> CalculateAsync(Guid packageId, string? promoCode);
    Task<ResponseModel<PricingResultDto>> PreviewAsync(Guid packageId);
}
