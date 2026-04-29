using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace divoom.app.Services;

public sealed class DallEProvider : IImageGenerationProvider
{
    public string DisplayName => "ChatGPT (DALL-E 3)";
    public string SettingsKey => "ApiKey_DallE";

    public async Task<byte[]> GenerateAsync(string prompt, string apiKey, CancellationToken ct = default)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var body = JsonSerializer.Serialize(new
        {
            model = "dall-e-3",
            prompt,
            n = 1,
            size = "1024x1024",
            response_format = "b64_json"
        });

        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.openai.com/v1/images/generations", content, ct);
        if (!response.IsSuccessStatusCode)
            throw new ImageGenerationException(
                $"DALL-E returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(ct)}");

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var b64 = doc.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString()
                  ?? throw new ImageGenerationException("DALL-E: no b64_json in response");

        return Convert.FromBase64String(b64);
    }
}
