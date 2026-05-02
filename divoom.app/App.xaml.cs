using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace divoom.app;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    internal static MainWindow MainWindow { get; private set; } = null!;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    private static async void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        var dialog = new ContentDialog
        {
            Title = "Unexpected error",
            Content = e.Exception?.Message ?? "An unknown error occurred.",
            CloseButtonText = "OK",
            XamlRoot = MainWindow?.Content?.XamlRoot
        };
        try { await dialog.ShowAsync(); } catch { }
    }
}