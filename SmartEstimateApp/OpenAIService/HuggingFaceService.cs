using Microsoft.Extensions.Options;
using OpenAIService.Interfaces;
using OpenAIService.Models;
using OpenAIService.Security;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OpenAIService
{
    public class HuggingFaceService : IOpenAiService
    {
        private readonly HttpClient _http;
        private readonly HfSettings _opts;
        private readonly string _apiKey;

        public HuggingFaceService(HttpClient http, IOptions<HfSettings> opts)
        {
            _http = http;
            _opts = opts.Value;
            _apiKey = ApiKeyEncryptor.LoadApiKey()
                    ?? throw new InvalidOperationException("HF API key not found.");

            _http.BaseAddress = new Uri(_opts.BaseUrl);
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> AskAsync(string prompt)
        {
            var payload = new
            {
                inputs = prompt,
                parameters = new { max_new_tokens = _opts.MaxTokens }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            string path = $"/models/{_opts.ModelId}";

            using var resp = await _http.PostAsync(path, content);

            if (resp.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException(
                    $"Model “{_opts.ModelId}” не найден по адресу {_opts.BaseUrl}{path}."
                );

            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement[0]
                       .GetProperty("generated_text")
                       .GetString()!
                       .Trim();
        }
    }
}