using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Startup_Project.Application.Abstractions.AI;

namespace Startup_Project.Infrastructure.Concretes.Services.AI;

public class OpenAiProvider(
    HttpClient httpClient,
    IOptions<OpenAiSettings> options,
    ILogger<OpenAiProvider> logger) : IAiProvider
{
    private readonly OpenAiSettings _settings = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<AiProviderResponse> SendAsync(List<AiChatMessage> messages, CancellationToken ct = default)
    {
        try
        {
            var requestBody = new OpenAiRequest
            {
                Model = _settings.Model,
                Messages = messages.Select(m => new OpenAiMessageDto
                {
                    Role = m.Role,
                    Content = m.Content
                }).ToList(),
                MaxTokens = _settings.MaxTokens,
                Temperature = _settings.Temperature
            };

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

            using var response = await httpClient.PostAsJsonAsync(
                "https://api.openai.com/v1/chat/completions",
                requestBody,
                JsonOptions,
                ct);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, ct);
                using var retryResponse = await httpClient.PostAsJsonAsync(
                    "https://api.openai.com/v1/chat/completions",
                    requestBody,
                    JsonOptions,
                    ct);

                if (!retryResponse.IsSuccessStatusCode)
                {
                    logger.LogWarning("OpenAI rate limit retry failed: {Status}", retryResponse.StatusCode);
                    return new AiProviderResponse
                    {
                        Success = false,
                        Error = "AI xidm?ti hal-haz?rda çox yüklüdür. Bir az sonra yenid?n c?hd edin."
                    };
                }

                return await ParseResponseAsync(retryResponse, ct);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning("OpenAI API error: {Status} {Body}", response.StatusCode, errorBody);
                return new AiProviderResponse
                {
                    Success = false,
                    Error = "AI xidm?ti müv?qq?ti ?lçatmazd?r. Yenid?n c?hd edin."
                };
            }

            return await ParseResponseAsync(response, ct);
        }
        catch (TaskCanceledException)
        {
            return new AiProviderResponse
            {
                Success = false,
                Error = "Cavab çox uzun ç?kdi. Yenid?n c?hd edin."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OpenAI API unexpected error");
            return new AiProviderResponse
            {
                Success = false,
                Error = "AI xidm?tind? x?ta ba? verdi. Yenid?n c?hd edin."
            };
        }
    }

    private static async Task<AiProviderResponse> ParseResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<OpenAiResponse>(json, JsonOptions);

        var content = result?.Choices?.FirstOrDefault()?.Message?.Content;
        var tokensUsed = result?.Usage?.TotalTokens ?? 0;

        if (string.IsNullOrWhiteSpace(content))
        {
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

    #region ========== OpenAI JSON Models ==========

    private class OpenAiRequest
    {
        public string Model { get; set; } = null!;
        public List<OpenAiMessageDto> Messages { get; set; } = [];
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
    }

    private class OpenAiMessageDto
    {
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    private class OpenAiResponse
    {
        public List<OpenAiChoice>? Choices { get; set; }
        public OpenAiUsage? Usage { get; set; }
    }

    private class OpenAiChoice
    {
        public OpenAiMessageDto? Message { get; set; }
    }

    private class OpenAiUsage
    {
        public int TotalTokens { get; set; }
    }

    #endregion
}
