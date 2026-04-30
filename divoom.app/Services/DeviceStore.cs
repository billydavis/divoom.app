using divoom.app.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace divoom.app.Services;

public static class DeviceStore
{
    private const string FileName = "devices.json";
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = false };

    public static async Task<List<DeviceViewModel>> LoadAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var item = await folder.TryGetItemAsync(FileName);
            if (item is not StorageFile file) return [];

            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<DeviceViewModel>>(json, _options) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static async Task SaveAsync(IEnumerable<DeviceViewModel> devices)
    {
        try
        {
            var json = JsonSerializer.Serialize(devices, _options);
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DeviceStore.SaveAsync failed: {ex.Message}");
        }
    }
}
