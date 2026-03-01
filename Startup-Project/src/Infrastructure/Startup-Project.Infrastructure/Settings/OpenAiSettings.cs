namespace Startup_Project.Infrastructure;

public class OpenAiSettings
{
    public const string SectionName = "OpenAI";
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = "gpt-4o-mini";
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;
}
