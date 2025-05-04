using System.Collections.Generic;
using Divoom.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace divoom.app.Models;

public class DeviceViewModel : INotifyPropertyChanged
{
    public DeviceViewModel()
    {

    }

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
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
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
                OnPropertyChanged(nameof(Hardware));
            }
        }
    }

    private string _ipAddress = string.Empty;
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            if (_ipAddress != value)
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }
    }

    private string _macAddress = string.Empty;
    public string MacAddress
    {
        get => _macAddress;
        set
        {
            if (_macAddress != value)
            {
                _macAddress = value;
                OnPropertyChanged(nameof(MacAddress));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}