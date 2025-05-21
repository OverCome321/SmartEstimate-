namespace OpenAIService.Interfaces;

public interface IOpenAiService
{
    Task<string> AskAsync(string prompt);
}
