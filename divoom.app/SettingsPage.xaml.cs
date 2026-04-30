using divoom.app.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace divoom.app;

public sealed partial class SettingsPage : Page
{
    private readonly List<(string Token, string DisplayPath)> _folderItems = new();
    private bool _loading;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _loading = true;
        try
        {
            ProbeOnStartupToggle.IsOn = AppSettings.ProbeOnStartup;

            _folderItems.Clear();
            foreach (var token in AppSettings.ExtraFolderTokens)
            {
                string displayPath;
                try
                {
                    var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                    displayPath = folder.Path;
                }
                catch
                {
                    displayPath = "(folder unavailable)";
                }
                _folderItems.Add((token, displayPath));
            }
            RefreshFolderList();

            DallEApiKeyBox.Password = AppSettings.GetApiKey("ApiKey_DallE") ?? "";
            GeminiApiKeyBox.Password = AppSettings.GetApiKey("ApiKey_Gemini") ?? "";
            DeepAiApiKeyBox.Password = AppSettings.GetApiKey("ApiKey_DeepAI") ?? "";
            StabilityApiKeyBox.Password = AppSettings.GetApiKey("ApiKey_Stability") ?? "";

            GlobalInstructionsBox.Text = AppSettings.GlobalPromptInstructions;
            DallEInstructionsBox.Text = AppSettings.GetProviderInstructions("ApiKey_DallE") ?? "";
            GeminiInstructionsBox.Text = AppSettings.GetProviderInstructions("ApiKey_Gemini") ?? "";
            DeepAiInstructionsBox.Text = AppSettings.GetProviderInstructions("ApiKey_DeepAI") ?? "";
            StabilityInstructionsBox.Text = AppSettings.GetProviderInstructions("ApiKey_Stability") ?? "";
        }
        finally
        {
            _loading = false;
        }
    }

    private void RefreshFolderList()
    {
        FolderListPanel.Children.Clear();
        foreach (var (token, displayPath) in _folderItems)
        {
            var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var label = new TextBlock
            {
                Text = displayPath,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis,
                FontSize = 12,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(label, 0);

            var removeBtn = new Button
            {
                Content = "Remove",
                Tag = token,
                FontSize = 12,
                Padding = new Thickness(8, 4, 8, 4)
            };
            removeBtn.Click += RemoveFolder_Click;
            Grid.SetColumn(removeBtn, 1);

            grid.Children.Add(label);
            grid.Children.Add(removeBtn);
            FolderListPanel.Children.Add(grid);
        }
    }

    private void ProbeOnStartupToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        AppSettings.ProbeOnStartup = ProbeOnStartupToggle.IsOn;
    }

    private async void AddFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.Desktop };
        picker.FileTypeFilter.Add("*");
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) return;

        var token = StorageApplicationPermissions.FutureAccessList.Add(folder);
        AppSettings.AddFolderToken(token);
        _folderItems.Add((token, folder.Path));
        RefreshFolderList();
    }

    private void RemoveFolder_Click(object sender, RoutedEventArgs e)
    {
        var token = (string)((Button)sender).Tag;
        try { StorageApplicationPermissions.FutureAccessList.Remove(token); } catch { }
        AppSettings.RemoveFolderToken(token);
        _folderItems.RemoveAll(f => f.Token == token);
        RefreshFolderList();
    }

    private void RevealToggle_Click(object sender, RoutedEventArgs e)
    {
        var toggle = (ToggleButton)sender;
        var grid = (Grid)toggle.Parent;
        foreach (var child in grid.Children)
        {
            if (child is PasswordBox pb)
            {
                pb.PasswordRevealMode = toggle.IsChecked == true
                    ? PasswordRevealMode.Visible
                    : PasswordRevealMode.Hidden;
                ((FontIcon)toggle.Content).Glyph = toggle.IsChecked == true
                    ? ""   // Hide (eye crossed)
                    : ""; // View (eye open)
                break;
            }
        }
    }

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        var box = (PasswordBox)sender;
        AppSettings.SetApiKey((string)box.Tag, box.Password);
    }

    private void GlobalInstructionsBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        AppSettings.GlobalPromptInstructions = GlobalInstructionsBox.Text;
    }

    private void ProviderInstructionsBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        var box = (TextBox)sender;
        AppSettings.SetProviderInstructions((string)box.Tag, box.Text);
    }
}
