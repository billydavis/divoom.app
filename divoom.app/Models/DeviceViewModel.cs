using System.Collections.Generic;
using Divoom.Models;
using divoom.app.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace divoom.app.Models;

public class DeviceViewModel : INotifyPropertyChanged
{
    public DeviceViewModel() { }

    public DeviceViewModel(DeviceInfo info)
    {
        Id = info.Id;
        Hardware = info.Hardware;
        Name = info.Name;
        IpAddress = info.IpAddress;
        MacAddress = info.MacAddress;
    }

    private int? _id;
    public int? Id
    {
        get => _id;
        set { if (_id != value) { _id = value; OnPropertyChanged(); } }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; OnPropertyChanged(); } }
    }

    private int? _hardware;
    public int? Hardware
    {
        get => _hardware;
        set
        {
            if (_hardware != value)
            {
                _hardware = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChannelCount));
                OnPropertyChanged(nameof(HasMultipleChannels));
            }
        }
    }

    private string _ipAddress = string.Empty;
    public string IpAddress
    {
        get => _ipAddress;
        set { if (_ipAddress != value) { _ipAddress = value; OnPropertyChanged(); } }
    }

    private string _macAddress = string.Empty;
    public string MacAddress
    {
        get => _macAddress;
        set { if (_macAddress != value) { _macAddress = value; OnPropertyChanged(); } }
    }

    private bool _isOnline;
    [JsonIgnore]
    public bool IsOnline
    {
        get => _isOnline;
        set { if (_isOnline != value) { _isOnline = value; OnPropertyChanged(); } }
    }

    private bool _isSelected;
    [JsonIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
    }

    private int _selectedChannel;
    public int SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            if (_selectedChannel != value)
            {
                _selectedChannel = value;
                OnPropertyChanged();
                AppState.NotifyChannelChanged(this);
            }
        }
    }

    [JsonIgnore]
    public int ChannelCount => Hardware == 400 ? 5 : 1;

    [JsonIgnore]
    public bool HasMultipleChannels => ChannelCount > 1;

    public DeviceInfo ToDeviceInfo() => new()
    {
        Id = Id,
        Hardware = Hardware,
        Name = Name,
        IpAddress = IpAddress,
        MacAddress = MacAddress
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
