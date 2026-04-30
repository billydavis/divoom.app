using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace divoom.app.Services;

public static class AppSettings
{
    private static readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    private const string KeyProbeOnStartup = "Settings_ProbeOnStartup";
    private const string KeyExtraFolderTokens = "Settings_ExtraFolderTokens";
    private const string KeyGlobalPromptInstructions = "Settings_GlobalPromptInstructions";
    private const string KeyProviderInstructionsPrefix = "Settings_ProviderInstructions_";
    private const string KeyLastSelectedMac = "Settings_LastSelectedMac";
    private const string KeyLastSelectedChannel = "Settings_LastSelectedChannel";

    public static string? LastSelectedMac
    {
        get => _settings.Values[KeyLastSelectedMac] as string;
        set
        {
            if (value is null) _settings.Values.Remove(KeyLastSelectedMac);
            else _settings.Values[KeyLastSelectedMac] = value;
        }
    }

    public static int LastSelectedChannel
    {
        get => int.TryParse(_settings.Values[KeyLastSelectedChannel] as string, out int ch) ? ch : 0;
        set => _settings.Values[KeyLastSelectedChannel] = value.ToString();
    }

    public static bool ProbeOnStartup
    {
        get => !string.Equals(_settings.Values[KeyProbeOnStartup] as string, "false", System.StringComparison.OrdinalIgnoreCase);
        set => _settings.Values[KeyProbeOnStartup] = value ? "true" : "false";
    }

    public static IReadOnlyList<string> ExtraFolderTokens
    {
        get
        {
            var raw = _settings.Values[KeyExtraFolderTokens] as string ?? "";
            return string.IsNullOrEmpty(raw) ? [] : raw.Split('|');
        }
    }

    public static void AddFolderToken(string token)
    {
        var tokens = ExtraFolderTokens.ToList();
        if (!tokens.Contains(token))
        {
            tokens.Add(token);
            _settings.Values[KeyExtraFolderTokens] = string.Join('|', tokens);
        }
    }

    public static void RemoveFolderToken(string token)
    {
        var tokens = ExtraFolderTokens.ToList();
        tokens.Remove(token);
        _settings.Values[KeyExtraFolderTokens] = string.Join('|', tokens);
    }

    public static string? GetApiKey(string settingsKey) =>
        _settings.Values[settingsKey] as string;

    public static void SetApiKey(string settingsKey, string value) =>
        _settings.Values[settingsKey] = value;

    public static string GlobalPromptInstructions
    {
        get => _settings.Values[KeyGlobalPromptInstructions] as string ?? "";
        set => _settings.Values[KeyGlobalPromptInstructions] = value;
    }

    public static string? GetProviderInstructions(string settingsKey) =>
        _settings.Values[KeyProviderInstructionsPrefix + settingsKey] as string;

    public static void SetProviderInstructions(string settingsKey, string value) =>
        _settings.Values[KeyProviderInstructionsPrefix + settingsKey] = value;

    public static string BuildPrompt(string userPrompt, string providerSettingsKey)
    {
        var parts = new List<string>();
        var global = GlobalPromptInstructions.Trim();
        if (!string.IsNullOrEmpty(global)) parts.Add(global);
        var provider = (GetProviderInstructions(providerSettingsKey) ?? "").Trim();
        if (!string.IsNullOrEmpty(provider)) parts.Add(provider);
        parts.Add(userPrompt.Trim());
        return string.Join("\n", parts);
    }
}
