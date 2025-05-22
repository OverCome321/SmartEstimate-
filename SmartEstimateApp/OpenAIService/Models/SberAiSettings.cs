namespace OpenAIService.Models;

public class SberAiSettings
{
    public string OAuthUrl { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public string ModelId { get; set; } = null!;
    public string Scope { get; set; } = null!;
    public string GrantType { get; set; } = "client_credentials";
    public string SystemPrompt { get; set; } = null!;
    public List<FewShotExample> FewShotExamples { get; set; } = new();
}
