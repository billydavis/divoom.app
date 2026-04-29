using divoom.app.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace divoom.app;

public sealed partial class CreateImagePage : Page
{
    private static readonly IReadOnlyList<IImageGenerationProvider> _providers =
        new List<IImageGenerationProvider>
        {
            new DeepAiProvider(),
            new DallEProvider(),
            new GeminiProvider(),
            new StabilityProvider()
        };

    private IImageGenerationProvider _activeProvider = _providers[0];
    private byte[]? _generatedBytes;
    private CancellationTokenSource? _cts;

    public CreateImagePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var p in _providers)
            ProviderComboBox.Items.Add(p.DisplayName);
        ProviderComboBox.SelectedIndex = 0;
        ApiKeyBox.Password = ApiKeyStore.Load(_activeProvider.SettingsKey) ?? string.Empty;
    }

    private void ProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _activeProvider = _providers[ProviderComboBox.SelectedIndex];
        ApiKeyBox.Password = ApiKeyStore.Load(_activeProvider.SettingsKey) ?? string.Empty;
    }

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ApiKeyBox.Password))
            ApiKeyStore.Save(_activeProvider.SettingsKey, ApiKeyBox.Password);
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var prompt = PromptBox.Text.Trim();
        if (string.IsNullOrEmpty(prompt)) return;

        var apiKey = ApiKeyBox.Password.Trim();
        if (string.IsNullOrEmpty(apiKey))
        {
            ShowError("Please enter an API key for the selected provider.");
            return;
        }

        GenerateButton.IsEnabled = false;
        ClearButton.IsEnabled = false;
        GeneratingRing.IsActive = true;
        GeneratingRing.Visibility = Visibility.Visible;
        ErrorPanel.Visibility = Visibility.Collapsed;
        SaveConfirmBar.IsOpen = false;
        PreviewBorder.Visibility = Visibility.Collapsed;
        SaveButton.Visibility = Visibility.Collapsed;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            _generatedBytes = await _activeProvider.GenerateAsync(prompt, apiKey, _cts.Token);

            using var ms = new InMemoryRandomAccessStream();
            await ms.WriteAsync(_generatedBytes.AsBuffer());
            ms.Seek(0);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(ms);

            PreviewImage.Source = bitmap;
            PreviewBorder.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Visible;
        }
        catch (OperationCanceledException) { }
        catch (ImageGenerationException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Unexpected error: {ex.Message}");
        }
        finally
        {
            GenerateButton.IsEnabled = true;
            ClearButton.IsEnabled = true;
            GeneratingRing.IsActive = false;
            GeneratingRing.Visibility = Visibility.Collapsed;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        _generatedBytes = null;
        PromptBox.Text = string.Empty;
        PreviewImage.Source = null;
        PreviewBorder.Visibility = Visibility.Collapsed;
        SaveButton.Visibility = Visibility.Collapsed;
        ErrorPanel.Visibility = Visibility.Collapsed;
        SaveConfirmBar.IsOpen = false;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_generatedBytes is null) return;

        try
        {
            var imagesFolder = await ApplicationData.Current.LocalFolder
                .CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

            var fileName = $"ai_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var file = await imagesFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

            using var ms = new InMemoryRandomAccessStream();
            await ms.WriteAsync(_generatedBytes.AsBuffer());
            ms.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(ms);
            var softBitmap = await decoder.GetSoftwareBitmapAsync();
            encoder.SetSoftwareBitmap(softBitmap);
            await encoder.FlushAsync();

            SaveConfirmBar.Message = $"Saved as {fileName}";
            SaveConfirmBar.IsOpen = true;
            SaveButton.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            ShowError($"Save failed: {ex.Message}");
        }
    }

    private void ErrorDismiss_Click(object sender, RoutedEventArgs e)
    {
        ErrorPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ErrorTextBox.Text = message;
        ErrorPanel.Visibility = Visibility.Visible;
    }
}
