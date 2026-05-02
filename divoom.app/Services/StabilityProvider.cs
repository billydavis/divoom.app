using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace divoom.app.Services;

public sealed class StabilityProvider : IImageGenerationProvider
{
    public string DisplayName => "Stability AI";
    public string SettingsKey => "ApiKey_Stability";

    public async Task<byte[]> GenerateAsync(string prompt, string apiKey, CancellationToken ct = default)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("Accept", "image/*");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(prompt), "prompt");
        form.Add(new StringContent("png"), "output_format");
        form.Add(new StringContent("512"), "width");
        form.Add(new StringContent("512"), "height");

        var response = await client.PostAsync(
            "https://api.stability.ai/v2beta/stable-image/generate/core",
            form, ct);

        if (!response.IsSuccessStatusCode)
            throw new ImageGenerationException(
                $"Stability AI returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(ct)}");

        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}
