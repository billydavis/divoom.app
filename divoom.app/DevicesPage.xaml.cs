using Divoom;
using divoom.app.Models;
using divoom.app.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace divoom.app;

public sealed partial class DevicesPage : Page, INotifyPropertyChanged
{
    public ObservableCollection<DeviceViewModel> Devices { get; } = new();

    private bool _isScanning;
    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            _isScanning = value;
            OnPropertyChanged(nameof(IsScanning));
            OnPropertyChanged(nameof(IsNotScanning));
            OnPropertyChanged(nameof(ScanButtonTextVisibility));
            OnPropertyChanged(nameof(ScanningIndicatorVisibility));
            OnPropertyChanged(nameof(ScanningPlaceholderVisibility));
            OnPropertyChanged(nameof(EmptyStateVisibility));
            OnPropertyChanged(nameof(DeviceListVisibility));
        }
    }

    public bool IsNotScanning => !_isScanning;
    public Visibility ScanButtonTextVisibility      => _isScanning ? Visibility.Collapsed : Visibility.Visible;
    public Visibility ScanningIndicatorVisibility   => _isScanning ? Visibility.Visible   : Visibility.Collapsed;
    public Visibility ScanningPlaceholderVisibility => _isScanning && Devices.Count == 0  ? Visibility.Visible : Visibility.Collapsed;
    public Visibility EmptyStateVisibility          => !_isScanning && Devices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DeviceListVisibility          => Devices.Count > 0                  ? Visibility.Visible : Visibility.Collapsed;

    private bool _isFirstScan = true;

    public DevicesPage()
    {
        this.InitializeComponent();
        Devices.CollectionChanged += (_, _) => NotifyDeviceCountChanged();
        DivoomButton.RemoveRequested += OnDeviceRemoveRequested;

        if (AppSettings.ProbeOnStartup)
            _ = ScanAsync();
    }

    private void NotifyDeviceCountChanged()
    {
        OnPropertyChanged(nameof(ScanningPlaceholderVisibility));
        OnPropertyChanged(nameof(EmptyStateVisibility));
        OnPropertyChanged(nameof(DeviceListVisibility));
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e) => await ScanAsync();

    private async System.Threading.Tasks.Task ScanAsync()
    {
        if (_isScanning) return;
        IsScanning = true;

        // Load persisted devices on first scan (shows them as offline immediately)
        if (Devices.Count == 0)
        {
            var saved = await DeviceStore.LoadAsync();
            foreach (var d in saved)
            {
                d.IsOnline = false;
                Devices.Add(d);
            }
        }

        try
        {
            var found = await Service.FindDevices();
            foreach (var info in found)
            {
                var existing = Devices.FirstOrDefault(d =>
                    !string.IsNullOrEmpty(d.MacAddress) &&
                    d.MacAddress == info.MacAddress);

                if (existing is not null)
                {
                    existing.IpAddress = info.IpAddress ?? existing.IpAddress;
                    existing.IsOnline = true;
                }
                else
                {
                    Devices.Add(new DeviceViewModel(info) { IsOnline = true });
                }
            }
        }
        catch { }
        finally
        {
            IsScanning = false;
            await DeviceStore.SaveAsync(Devices);

            // Restore last selection on startup only, not on manual re-scans
            if (_isFirstScan)
            {
                _isFirstScan = false;
                var lastMac = AppSettings.LastSelectedMac;
                if (lastMac is not null && AppState.SelectedDevice is null)
                {
                    var match = Devices.FirstOrDefault(d => d.MacAddress == lastMac);
                    if (match is not null)
                    {
                        match.SelectedChannel = AppSettings.LastSelectedChannel;
                        AppState.SelectDevice(match);
                    }
                }
            }
        }
    }

    private async void OnDeviceRemoveRequested(DeviceViewModel device)
    {
        if (AppState.SelectedDevice == device)
            AppState.SelectDevice(null);
        Devices.Remove(device);
        await DeviceStore.SaveAsync(Devices);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
