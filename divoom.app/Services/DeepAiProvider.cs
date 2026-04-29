using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace divoom.app.Services;

public sealed class DeepAiProvider : IImageGenerationProvider
{
    public string DisplayName => "DeepAI";
    public string SettingsKey => "ApiKey_DeepAI";

    public async Task<byte[]> GenerateAsync(string prompt, string apiKey, CancellationToken ct = default)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", apiKey);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(prompt), "text");

        var response = await client.PostAsync("https://api.deepai.org/api/text2img", form, ct);
        if (!response.IsSuccessStatusCode)
            throw new ImageGenerationException(
                $"DeepAI returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(ct)}");

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var url = doc.RootElement.GetProperty("output_url").GetString()
                  ?? throw new ImageGenerationException("DeepAI: no output_url in response");

        return await client.GetByteArrayAsync(url, ct);
    }
}
