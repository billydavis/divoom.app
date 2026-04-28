using divoom.app.Models;
using Microsoft.UI;
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
            UpdateTitleBarButtonColors();
            ((FrameworkElement)Content).ActualThemeChanged += (_, _) => UpdateTitleBarButtonColors();
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
        
    private void UpdateTitleBarButtonColors()
    {
        var titleBar = AppWindow.TitleBar;
        bool isDark = ((FrameworkElement)Content).ActualTheme == ElementTheme.Dark;

        titleBar.ButtonBackgroundColor         = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        if (isDark)
        {
            titleBar.ButtonForegroundColor        = Windows.UI.Color.FromArgb(0xFF, 0xE6, 0xED, 0xF3);
            titleBar.ButtonHoverForegroundColor   = Windows.UI.Color.FromArgb(0xFF, 0xE6, 0xED, 0xF3);
            titleBar.ButtonHoverBackgroundColor   = Windows.UI.Color.FromArgb(0xFF, 0x2D, 0x33, 0x3B);
            titleBar.ButtonPressedForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0xE6, 0xED, 0xF3);
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0x25, 0x2D, 0x37);
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(0x66, 0xE6, 0xED, 0xF3);
        }
        else
        {
            titleBar.ButtonForegroundColor        = Windows.UI.Color.FromArgb(0xFF, 0x1F, 0x23, 0x28);
            titleBar.ButtonHoverForegroundColor   = Windows.UI.Color.FromArgb(0xFF, 0x1F, 0x23, 0x28);
            titleBar.ButtonHoverBackgroundColor   = Windows.UI.Color.FromArgb(0xFF, 0xD1, 0xD6, 0xDC);
            titleBar.ButtonPressedForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0x1F, 0x23, 0x28);
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0xC4, 0xC9, 0xCF);
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(0x66, 0x1F, 0x23, 0x28);
        }
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