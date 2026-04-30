using Divoom;
using divoom.app.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using divoom.app.Models;
using Divoom.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace divoom.app;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DevicesPage : Page
{
    public ObservableCollection<DeviceViewModel> Devices { get; } =
        new ObservableCollection<DeviceViewModel>();

    public DevicesPage()
    {
        this.InitializeComponent();

        if (AppSettings.ProbeOnStartup)
            LoadDevices();
            
    }

    private async void  LoadDevices()
    {
        try
        {
            var devices = await Service.FindDevices();
            foreach (var device in devices)
            {
                
                    Devices.Add(new DeviceViewModel(device));
              
            }

            Console.WriteLine($"Found {Devices.Count} devices");
        }
        catch
        {
            Console.WriteLine("No devices found");
        }
    }

    private void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        Devices.Clear();
        LoadDevices();
    }

    private void DevicesGridContainerContentChanging(
        ListViewBase sender,
        ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            // var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
            // var image = templateRoot.FindName("ItemImage") as Image;
            // image.Source = null;
        }

        if (args.Phase == 0)
        {
            // args.RegisterUpdateCallback(ShowImage);
            args.Handled = true;
        }
    }

}