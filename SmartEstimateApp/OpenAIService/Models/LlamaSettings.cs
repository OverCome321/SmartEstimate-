namespace OpenAIService.Models;

public class LlamaSettings
{
    public string ModelPath { get; set; }
    public string SystemInstruction { get; set; }
    public int ContextSize { get; set; }
    public int Threads { get; set; }
    public int MaxTokens { get; set; }
}