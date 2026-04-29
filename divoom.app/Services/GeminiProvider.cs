using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace divoom.app.Services;

public sealed class GeminiProvider : IImageGenerationProvider
{
    public string DisplayName => "Google Imagen 4";
    public string SettingsKey => "ApiKey_Gemini";

    public async Task<byte[]> GenerateAsync(string prompt, string apiKey, CancellationToken ct = default)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        var body = JsonSerializer.Serialize(new
        {
            instances = new[] { new { prompt } },
            parameters = new { sampleCount = 1 }
        });

        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(
            "https://generativelanguage.googleapis.com/v1beta/models/imagen-4.0-generate-001:predict",
            content, ct);

        if (!response.IsSuccessStatusCode)
            throw new ImageGenerationException(
                $"Google Imagen returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(ct)}");

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Shape 1: { "predictions": [ { "bytesBase64Encoded": "...", "mimeType": "..." } ] }
        if (root.TryGetProperty("predictions", out var predictions) &&
            predictions.GetArrayLength() > 0 &&
            predictions[0].TryGetProperty("bytesBase64Encoded", out var encoded))
        {
            var b64 = encoded.GetString()
                ?? throw new ImageGenerationException("Google Imagen: bytesBase64Encoded was null");
            return Convert.FromBase64String(b64);
        }

        // Shape 2: { "generatedImages": [ { "image": { "imageBytes": "..." } } ] }
        if (root.TryGetProperty("generatedImages", out var generatedImages) &&
            generatedImages.GetArrayLength() > 0 &&
            generatedImages[0].TryGetProperty("image", out var image) &&
            image.TryGetProperty("imageBytes", out var imageBytes))
        {
            var b64 = imageBytes.GetString()
                ?? throw new ImageGenerationException("Google Imagen: imageBytes was null");
            return Convert.FromBase64String(b64);
        }

        throw new ImageGenerationException($"Google Imagen: unrecognised response shape — {json}");
    }
}
