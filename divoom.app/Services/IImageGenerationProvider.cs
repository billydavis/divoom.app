using System;
using System.Threading;
using System.Threading.Tasks;

namespace divoom.app.Services;

public interface IImageGenerationProvider
{
    string DisplayName { get; }
    string SettingsKey { get; }
    Task<byte[]> GenerateAsync(string prompt, string apiKey, CancellationToken ct = default);
}

public sealed class ImageGenerationException : Exception
{
    public ImageGenerationException(string message) : base(message) { }
    public ImageGenerationException(string message, Exception inner) : base(message, inner) { }
}
