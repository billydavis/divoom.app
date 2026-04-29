using Windows.Storage;

namespace divoom.app.Services;

public static class ApiKeyStore
{
    private static readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    public static string? Load(string key) =>
        _settings.Values.TryGetValue(key, out var v) ? v as string : null;

    public static void Save(string key, string value) =>
        _settings.Values[key] = value;
}
