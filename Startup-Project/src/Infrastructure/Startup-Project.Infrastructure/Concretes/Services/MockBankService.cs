using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Interfaces;

namespace Startup_Project.Infrastructure.Concretes.Services;

/// <summary>
/// KapitalBank ?v?zin? i?l?y?n mock bank servisi.
/// Real bank olmadan öd?ni? ax?n?n? tam test etm?k üçün.
/// appsettings.json ? "KapitalBank:UseMock": true
/// </summary>
public class MockBankService(
    IConfiguration config,
    ILogger<MockBankService> logger) : IBankService
{
    private static readonly Dictionary<string, MockPaymentSession> Sessions = new();
    private static readonly Lock Lock = new();

    public Task<PaymentResponseDto> CreateSessionAsync(decimal amount, string packageName, string callbackPath = "/api/payment/package-callback")
    {
        var sessionId = "MOCK-" + Guid.NewGuid().ToString("N")[..20];
        var baseUrl = config["KapitalBank:MockBaseUrl"] ?? "http://localhost:5000";

        lock (Lock)
        {
            Sessions[sessionId] = new MockPaymentSession
            {
                SessionId = sessionId,
                Amount = amount,
                Description = packageName,
                Status = "PENDING",
                CallbackPath = callbackPath,
                CreatedAt = DateTime.UtcNow
            };
        }

        var redirectUrl = baseUrl + "/api/mock-payment/checkout?sessionId=" + sessionId;

        logger.LogInformation("MockBank: Session yarad?ld? — {SessionId}, {Amount} AZN, {Desc}",
            sessionId, amount, packageName);

        return Task.FromResult(new PaymentResponseDto(redirectUrl, sessionId));
    }

    public Task<bool> CheckPaymentStatusAsync(string sessionId)
    {
        lock (Lock)
        {
            if (Sessions.TryGetValue(sessionId, out var session))
            {
                logger.LogInformation("MockBank: Status={Status}, SessionId={SessionId}",
                    session.Status, sessionId);
                return Task.FromResult(session.Status == "APPROVED");
            }
        }

        // API restart olduqda in-memory session itirilir.
        // Mock bank oldu?u üçün callback-a çatm?? session-? APPROVED say?r?q.
        // Real bank-da bu hal olmayacaq çünki status bank API-d?n yoxlan?l?r.
        logger.LogWarning("MockBank: Session tap?lmad? (API restart?), auto-approve — {SessionId}", sessionId);
        return Task.FromResult(true);
    }

    public static MockPaymentSession? GetSession(string sessionId)
    {
        lock (Lock)
        {
            return Sessions.TryGetValue(sessionId, out var s) ? s : null;
        }
    }

        public static void ApproveSession(string sessionId)
        {
            lock (Lock)
            {
                if (Sessions.TryGetValue(sessionId, out var s))
                    s.Status = "APPROVED";
            }
        }

        public static void DeclineSession(string sessionId)
        {
            lock (Lock)
            {
                if (Sessions.TryGetValue(sessionId, out var s))
                    s.Status = "DECLINED";
            }
        }

        public static void SetCardInfo(string sessionId, string cardNumber, string cardHolder)
        {
            lock (Lock)
            {
                if (Sessions.TryGetValue(sessionId, out var s))
                {
                    s.MaskedCard = "****" + cardNumber.Replace(" ", "")[^4..];
                    s.CardHolder = cardHolder;
                }
            }
        }

        public static void RecoverSession(string sessionId, decimal amount, string description, string callbackPath)
        {
            lock (Lock)
            {
                if (!Sessions.ContainsKey(sessionId))
                {
                    Sessions[sessionId] = new MockPaymentSession
                    {
                        SessionId = sessionId,
                        Amount = amount,
                        Description = description,
                        Status = "PENDING",
                        CallbackPath = callbackPath,
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }
        }
    }

    public class MockPaymentSession
    {
        public string SessionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public string Status { get; set; } = "PENDING";
        public string CallbackPath { get; set; } = "/api/payment/package-callback";
        public string? MaskedCard { get; set; }
        public string? CardHolder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
