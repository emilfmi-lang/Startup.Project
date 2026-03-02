using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;

var googleAi = new GoogleAI(apiKey: "AIzaSyBabODRjRrGUWre0DtcyYgzNGPHPPIXl5c");
var model = googleAi.GenerativeModel(model: "gemini-2.5-flash");
var prompt = "[System Instructions]\nSen FitZone AI fitness komekcisin. Azerbaycan dilinde cavab ver.\n\n[User]\nMene ariqlamaq ucun mesq proqrami yaz";
var request = new GenerateContentRequest
{
    Contents = [new Content { Role = "user", Parts = [new Part { Text = prompt }] }],
    GenerationConfig = new GenerationConfig { MaxOutputTokens = 2000, Temperature = 0.7f }
};
try {
    var response = await model.GenerateContent(request).WaitAsync(new CancellationTokenSource(30000).Token);
   
    var text = response?.Text;
    if (string.IsNullOrWhiteSpace(text)) {
        var candidate = response?.Candidates?.FirstOrDefault();
        if (candidate?.Content?.Parts != null) {
            var parts = candidate.Content.Parts.Where(p => p.Thought is null or false).Where(p => !string.IsNullOrWhiteSpace(p.Text)).Select(p => p.Text).ToList();
            text = parts.Count > 0 ? string.Join("\n", parts) : null;
        }
    }
    if (!string.IsNullOrWhiteSpace(text)) {
        Console.WriteLine("SUCCESS (len=" + text.Length + "):");
        Console.WriteLine(text.Substring(0, Math.Min(500, text.Length)));
    } else { Console.WriteLine("EMPTY RESPONSE"); }
} catch (Exception ex) { Console.WriteLine("ERR: " + ex.GetType().Name + " " + ex.Message); }