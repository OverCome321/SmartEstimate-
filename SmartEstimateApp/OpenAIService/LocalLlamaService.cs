using LLama;
using LLama.Common;
using Microsoft.Extensions.Options;
using OpenAIService.Interfaces;
using OpenAIService.Models;
using System.Text;

namespace OpenAIService;

public class LocalLlamaService : IOpenAiService, IDisposable
{
    private readonly LLamaContext _context;
    private readonly LlamaSettings _opts;

    public LocalLlamaService(IOptions<LlamaSettings> options)
    {
        _opts = options.Value;
        var pars = new ModelParams(_opts.ModelPath)
        {
            ContextSize = (uint)_opts.ContextSize,
            Threads = _opts.Threads
        };
        var weights = LLamaWeights.LoadFromFile(pars);
        _context = weights.CreateContext(pars);
    }

    public async Task<string> AskAsync(string prompt)
    {
        string sys = _opts.SystemInstruction;

        string fullPrompt =
            $"{sys}\n\n" +
            $"Вопрос: {prompt}\n" +
            $"Ответ:";

        var executor = new InteractiveExecutor(_context);

        var inf = new InferenceParams { MaxTokens = _opts.MaxTokens };
        var sb = new StringBuilder();

        await foreach (var chunk in executor.InferAsync(fullPrompt, inf))
            sb.Append(chunk);

        return sb.ToString().Trim();
    }

    public void Dispose() => _context.Dispose();
}

