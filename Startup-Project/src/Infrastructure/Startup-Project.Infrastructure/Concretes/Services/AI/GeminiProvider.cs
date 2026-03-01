using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using Startup_Project.Application.Abstractions.AI;

namespace Startup_Project.Infrastructure.Concretes.Services.AI;

public class GeminiProvider(
    IOptions<GeminiSettings> options,
    ILogger<GeminiProvider> logger) : IAiProvider
{
    private readonly GeminiSettings _settings = options.Value;

    public async Task<AiProviderResponse> SendAsync(List<AiChatMessage> messages, CancellationToken ct = default)
    {
        try
        {
            var googleAi = new GoogleAI(apiKey: _settings.ApiKey);
            var model = googleAi.GenerativeModel(model: _settings.Model);

            var systemMessage = messages.FirstOrDefault(m => m.Role == "system");

            var chatMessages = messages
                .Where(m => m.Role != "system")
                .ToList();

            if (chatMessages.Count == 0)
            {
                return new AiProviderResponse
                {
                    Success = false,
                    Error = "Mesaj tap?lmad?."
                };
            }

            var fullPrompt = BuildFullPrompt(systemMessage?.Content, chatMessages);

            var request = new GenerateContentRequest
            {
                Contents = [new Content
                {
                    Role = "user",
                    Parts = [new Part { Text = fullPrompt }]
                }],
                GenerationConfig = new GenerationConfig
                {
                    MaxOutputTokens = _settings.MaxTokens,
                    Temperature = (float)_settings.Temperature
                }
            };

            // Timeout: max 30 saniy? gözl?, sonra x?ta qaytar
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            GenerateContentResponse? response;
            try
            {
                response = await model.GenerateContent(request).WaitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                logger.LogWarning("Gemini API timeout after 30 seconds");
                return new AiProviderResponse
                {
                    Success = false,
                    Error = "AI cavab verm?di (timeout). Yenid?n c?hd edin."
                };
            }

            var content = response?.Text;
            var tokensUsed = response?.UsageMetadata?.TotalTokenCount ?? 0;

            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogWarning("Gemini returned empty response. Candidates: {Candidates}",
                    response?.Candidates?.Count ?? 0);
                return new AiProviderResponse
                {
                    Success = false,
                    Error = "AI bo? cavab qaytard?. Yenid?n c?hd edin."
                };
            }

            return new AiProviderResponse
            {
                Content = content,
                TokensUsed = tokensUsed,
                Success = true
            };
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Gemini request was cancelled");
            return new AiProviderResponse
            {
                Success = false,
                Error = "Cavab çox uzun ç?kdi. Yenid?n c?hd edin."
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Gemini API HTTP error: {Status}", ex.StatusCode);

            var errorMsg = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.TooManyRequests =>
                    "AI xidm?tinin limiti dolub. Bir neç? d?qiq? sonra yenid?n c?hd edin.",
                System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden =>
                    "AI xidm?tinin konfiqurasiyas?nda x?ta var.",
                _ => "AI xidm?ti müv?qq?ti ?lçatmazd?r. Yenid?n c?hd edin."
            };

            return new AiProviderResponse
            {
                Success = false,
                Error = errorMsg
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini API unexpected error: {Type} {Message}", ex.GetType().Name, ex.Message);

            // Mscc.GenerativeAI 429/quota x?talar?n? öz exception tipi il? ata bil?r
            if (ex.Message.Contains("429") || ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase))
            {
                return new AiProviderResponse
                {
                    Success = false,
                    Error = "AI xidm?tinin limiti dolub. Bir neç? d?qiq? sonra yenid?n c?hd edin."
                };
            }

            return new AiProviderResponse
            {
                Success = false,
                Error = "AI xidm?tind? x?ta ba? verdi. Yenid?n c?hd edin."
            };
        }
    }

    private static string BuildFullPrompt(string? systemPrompt, List<AiChatMessage> chatMessages)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            parts.Add($"[System Instructions]\n{systemPrompt}\n");
        }

        foreach (var msg in chatMessages.Take(chatMessages.Count - 1))
        {
            var label = msg.Role == "assistant" ? "Assistant" : "User";
            parts.Add($"[{label}]\n{msg.Content}");
        }

        var lastMessage = chatMessages.Last();
        parts.Add($"[User]\n{lastMessage.Content}");

        parts.Add("\n[Assistant]\n");

        return string.Join("\n\n", parts);
    }
}
