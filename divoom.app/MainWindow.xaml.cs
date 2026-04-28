using divoom.app.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.Graphics;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace divoom.app;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{

    private readonly Dictionary<string, Page> _pages = new Dictionary<string, Page>();

    public MainWindow()
    {
        InitializeComponent();

        if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
        {
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
        }

        
        var windowManager = WindowManager.Get(this);
        windowManager.Height = 670;
        windowManager.Width = 470;
        windowManager.IsMaximizable = true;
        windowManager.IsResizable = true;

        windowManager.PersistenceId = "divoom.app:MainWindow";

        // Initial Navigation
        SideMenu_OnNavigationChange(this, new NavigationChangeEvent { Page = "Devices" });
    }
        
    private void SideMenu_OnNavigationChange(object? sender, NavigationChangeEvent e)
    {
        Page page = null;

        switch (e.Page)
        {
            case "Devices":
                if (!_pages.TryGetValue("Devices", out page))
                {
                    page = new DevicesPage();
                    _pages["Devices"] = page;
                }
                break;
            case "Images":
                if (!_pages.TryGetValue("Images", out page))
                {
                    page = new ImageViewerPage();
                    _pages["Images"] = page;
                }
                break;
            default:
                if (!_pages.TryGetValue("ComingSoon", out page))
                {
                    page = new ComingSoon();
                    _pages["ComingSoon"] = page;
                }
                break;
        }

        ContentFrame.Navigate(page.GetType());
    }
}