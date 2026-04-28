using System;
using divoom.app.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace divoom.app;

public sealed partial class SideMenu : UserControl
{
    public event EventHandler<NavigationChangeEvent> NavigationChange;

    public SideMenu()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Select the first nav item by default
        foreach (UIElement child in RootGrid.Children)
        {
            if (child is SideMenuButton first)
            {
                OnSideMenuButtonClicked(first);
                break;
            }
        }
    }

    public void OnSideMenuButtonClicked(SideMenuButton clicked)
    {
        foreach (UIElement child in RootGrid.Children)
        {
            if (child is SideMenuButton btn)
                btn.IsSelected = btn == clicked;
        }
        NavigationChange?.Invoke(this, new NavigationChangeEvent { Page = clicked.Text });
    }
}
