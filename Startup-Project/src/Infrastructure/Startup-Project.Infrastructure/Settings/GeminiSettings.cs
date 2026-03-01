namespace Startup_Project.Infrastructure;

public class GeminiSettings
{
    public const string SectionName = "Gemini";
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = "gemini-2.5-flash";
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;
}
