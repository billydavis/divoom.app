using divoom.app.Models;
using divoom.app.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;

namespace divoom.app;

public sealed partial class DivoomButton : UserControl, INotifyPropertyChanged
{
    public static event Action<DeviceViewModel>? RemoveRequested;

    private DeviceViewModel _device = new();

    public DeviceViewModel Device
    {
        get => _device;
        set
        {
            if (_device == value) return;
            _device.PropertyChanged -= OnDevicePropertyChanged;
            _device = value;
            _device.PropertyChanged += OnDevicePropertyChanged;
            OnPropertyChanged(nameof(Device));
            Bindings.Update();
        }
    }

    public DivoomButton()
    {
        this.InitializeComponent();
        _device.PropertyChanged += OnDevicePropertyChanged;
    }

    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(Bindings.Update);
    }

    // --- Computed properties for x:Bind ---

    public string OnlineStatusText => Device.IsOnline ? "Online" : "Offline";

    public SolidColorBrush OnlineDotColor => Device.IsOnline
        ? new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x3F, 0xB9, 0x50))  // green
        : new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x6E, 0x76, 0x81)); // grey

    public Brush SelectionOverlayBrush => Device.IsSelected
        ? (Brush)Application.Current.Resources["AppNavSelectedBrush"]
        : new SolidColorBrush(Colors.Transparent);

    public Visibility SelectionBarVisibility =>
        Device.IsSelected ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ChannelSelectorVisibility =>
        Device.HasMultipleChannels ? Visibility.Visible : Visibility.Collapsed;

    // Called by x:Bind function syntax in XAML
    public bool IsChannelSelected(int channel) => Device.IsSelected && Device.SelectedChannel == channel;

    // --- Event handlers ---

    private void CardButton_Click(object sender, RoutedEventArgs e)
    {
        AppState.SelectDevice(Device);
    }

    private void ChannelButton_Click(object sender, RoutedEventArgs e)
    {
        var btn = (ToggleButton)sender;
        var channel = int.Parse((string)btn.Tag);
        Device.SelectedChannel = channel;
        AppState.SelectDevice(Device);
        // Keep the clicked button checked — Bindings.Update restores correct state
        btn.IsChecked = true;
        Bindings.Update();
    }

    private void RemoveDevice_Click(object sender, RoutedEventArgs e)
    {
        RemoveRequested?.Invoke(Device);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
