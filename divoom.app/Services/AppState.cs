using divoom.app.Models;
using System;
using System.Linq;

namespace divoom.app.Services;

public static class AppState
{
    public static DeviceViewModel? SelectedDevice { get; private set; }

    public static int EffectiveChannel => SelectedDevice?.SelectedChannel ?? 0;

    public static event Action? SelectionChanged;

    public static void SelectDevice(DeviceViewModel? device)
    {
        if (SelectedDevice == device) return;
        if (SelectedDevice is not null) SelectedDevice.IsSelected = false;
        SelectedDevice = device;
        if (SelectedDevice is not null) SelectedDevice.IsSelected = true;
        AppSettings.LastSelectedMac = device?.MacAddress;
        if (device is not null) AppSettings.LastSelectedChannel = device.SelectedChannel;
        SelectionChanged?.Invoke();
    }

    public static void NotifyChannelChanged(DeviceViewModel device)
    {
        if (device != SelectedDevice) return;
        AppSettings.LastSelectedChannel = device.SelectedChannel;
        SelectionChanged?.Invoke();
    }
}
