using System;
using divoom.app.Models;
using divoom.app.Services;
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
        AppState.SelectionChanged += OnSelectionChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
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

    private void OnSelectionChanged()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var dev = AppState.SelectedDevice;
            ActiveTargetPanel.Visibility = dev is null ? Visibility.Collapsed : Visibility.Visible;
            if (dev is null) return;
            ActiveDeviceNameText.Text = dev.Name;
            ActiveChannelText.Text = dev.HasMultipleChannels
                ? $"Screen {AppState.EffectiveChannel + 1}"
                : "Screen 1";
        });
    }
}
