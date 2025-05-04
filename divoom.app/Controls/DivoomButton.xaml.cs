using divoom.app.Models;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace divoom.app;

public sealed partial class DivoomButton : UserControl, INotifyPropertyChanged
{
    private DeviceViewModel _device = new DeviceViewModel();

    public DeviceViewModel Device
    {
        get => _device;
        set
        {
            if (_device != value)
            {
                _device = value;

                OnPropertyChanged(nameof(Device));

                Bindings.Update();
            }
        }
    }

    public DivoomButton()
    {
        this.InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}