using Microsoft.Extensions.Options;
using OpenAIService.Interfaces;
using OpenAIService.Models;
using OpenAIService.Security;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OpenAIService;

public class SberAiService : IOpenAiService
{
    private readonly HttpClient _http;
    private readonly SberAiSettings _opts;
    private string? _accessToken;
    private DateTimeOffset _expiresAt;

    public SberAiService(HttpClient http, IOptions<SberAiSettings> opts)
    {
        _http = http;
        _opts = opts.Value;
        _http.BaseAddress = new Uri(_opts.BaseUrl);
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task RefreshTokenAsync()
    {
        var basicKey = SberApiKeyEncryptor.LoadApiKey()
                      ?? throw new InvalidOperationException("Sber API key not found.");

        using var req = new HttpRequestMessage(HttpMethod.Post, _opts.OAuthUrl);
        req.Headers.Add("RqUID", Guid.NewGuid().ToString());
        req.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", basicKey);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = _opts.GrantType,
            ["scope"] = _opts.Scope
        });

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();

        long expMillis = doc.RootElement.GetProperty("expires_at").GetInt64();
        _expiresAt = DateTimeOffset.FromUnixTimeMilliseconds(expMillis);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<string> AskAsync(string prompt)
    {
        if (_accessToken == null || DateTimeOffset.UtcNow >= _expiresAt)
            await RefreshTokenAsync();

        var messages = new List<object>
        {
            new { role = "system", content = _opts.SystemPrompt }
        };

        foreach (var ex in _opts.FewShotExamples)
        {
            messages.Add(new { role = "user", content = ex.User });
            messages.Add(new { role = "assistant", content = ex.Assistant });
        }

        messages.Add(new { role = "user", content = prompt });

        var payload = new
        {
            model = _opts.ModelId,
            messages = messages
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json"
        );

        var resp = await _http.PostAsync("/api/v1/chat/completions", content);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString()!;
    }

}
