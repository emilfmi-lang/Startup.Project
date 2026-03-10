using Microsoft.Extensions.Configuration;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class KapitalBankService : IBankService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public KapitalBankService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }
    public async Task<PaymentResponseDto> CreateSessionAsync(decimal amount, string packageName, string callbackPath = "/api/payment/package-callback")
    {
        var paymentRequest = new
        {
            merchantId = _config["KapitalBank:MerchantId"],
            amount = amount * 100, 
            currency = "944",    
            description = packageName,
            approveUrl = _config["KapitalBank:SuccessUrl"],
            cancelUrl = _config["KapitalBank:FailUrl"],
            language = "AZ"
        };

        var response = await _httpClient.PostAsJsonAsync("https://tpay.kapitalbank.az/api/v1/checkout", paymentRequest);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Bank servisi ilə əlaqə kəsildi.");

        var bankData = await response.Content.ReadFromJsonAsync<KapitalBankResponse>();

        return new PaymentResponseDto(bankData.RedirectUrl, bankData.SessionId);
    }
    public async Task<bool> CheckPaymentStatusAsync(string sessionId)
    {
        var merchantId = _config["KapitalBank:MerchantId"];
        var response = await _httpClient.GetAsync(
                    $"https://tpay.kapitalbank.az/api/v1/checkout/{sessionId}?merchantId={merchantId}");
        var result = await response.Content.ReadFromJsonAsync<KapitalBankResponse>();
        return result.Status == "APPROVED";
    }
}
