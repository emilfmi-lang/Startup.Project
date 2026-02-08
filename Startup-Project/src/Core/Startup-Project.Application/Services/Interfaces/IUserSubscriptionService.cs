using Startup_Project.Application.Dtos.Subscriptions;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IUserSubscriptionService
{
    /// <summary>
    /// Cari istifad?çinin aktiv abun?likl?rini g?tirir
    /// </summary>
    Task<ResponseModel<List<UserSubscriptionDto>>> GetMyActiveSubscriptionsAsync();

    /// <summary>
    /// Cari istifad?çinin öd?ni? tarixç?sini g?tirir
    /// </summary>
    Task<ResponseModel<List<SubscriptionHistoryDto>>> GetMyPaymentHistoryAsync(int page = 1, int pageSize = 20);


    /// <summary>
    /// Abun?liyi yenil?yir (renew)
    /// </summary>
    Task<ResponseModel<bool>> RenewSubscriptionAsync(Guid subscriptionId);
}
