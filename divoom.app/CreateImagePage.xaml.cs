using divoom.app.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace divoom.app;

public sealed partial class CreateImagePage : Page
{
    public static event Action<Windows.Storage.StorageFile>? ImageSaved;

    private static readonly IReadOnlyList<IImageGenerationProvider> _providers =
        new List<IImageGenerationProvider>
        {
            new DeepAiProvider(),
            new DallEProvider(),
            new GeminiProvider(),
            new StabilityProvider()
        };

    private IImageGenerationProvider _activeProvider = _providers[0];
    private CancellationTokenSource? _cts;

    public CreateImagePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ProviderComboBox.Items.Count == 0)
        {
            foreach (var p in _providers)
                ProviderComboBox.Items.Add(p.DisplayName);
            ProviderComboBox.SelectedIndex = 0;
        }
        PromptBox.Focus(FocusState.Programmatic);
    }

    private void ProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _activeProvider = _providers[ProviderComboBox.SelectedIndex];
    }

    private void PromptBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            _ = GenerateAsync();
        }
        else if (e.Key == Windows.System.VirtualKey.Escape)
        {
            e.Handled = true;
            PromptBox.Text = string.Empty;
        }
    }

    private async Task GenerateAsync()
    {
        var prompt = PromptBox.Text.Trim();
        if (string.IsNullOrEmpty(prompt)) return;

        var apiKey = AppSettings.GetApiKey(_activeProvider.SettingsKey) ?? string.Empty;
        if (string.IsNullOrEmpty(apiKey))
        {
            ShowError("No API key configured. Go to Settings to add one.");
            return;
        }

        GeneratingRing.IsActive = true;
        GeneratingRing.Visibility = Visibility.Visible;
        ErrorPanel.Visibility = Visibility.Collapsed;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            var bytes = await _activeProvider.GenerateAsync(
                AppSettings.BuildPrompt(prompt, _activeProvider.SettingsKey), apiKey, _cts.Token);

            using var ms = new InMemoryRandomAccessStream();
            await ms.WriteAsync(bytes.AsBuffer());
            ms.Seek(0);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(ms);

            var card = CreateImageCard(bytes, bitmap);
            ResultsPanel.Children.Add(card);

            ResultsPanel.UpdateLayout();
            ResultsScrollViewer.ScrollToVerticalOffset(ResultsScrollViewer.ScrollableHeight);
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
            GeneratingRing.IsActive = false;
            GeneratingRing.Visibility = Visibility.Collapsed;
        }
    }

    private FrameworkElement CreateImageCard(byte[] bytes, BitmapImage bitmap)
    {
        var image = new Image
        {
            Source = bitmap,
            MaxHeight = 320,
            Stretch = Stretch.Uniform
        };

        var saveButton = new Button
        {
            Content = "Save to Gallery",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 8, 0, 0)
        };

        saveButton.Click += async (s, e) =>
        {
            saveButton.IsEnabled = false;
            try
            {
                var imagesFolder = await ApplicationData.Current.LocalFolder
                    .CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

                var fileName = $"ai_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var file = await imagesFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

                using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                using var ms = new InMemoryRandomAccessStream();
                await ms.WriteAsync(bytes.AsBuffer());
                ms.Seek(0);
                var decoder = await BitmapDecoder.CreateAsync(ms);
                var softBitmap = await decoder.GetSoftwareBitmapAsync();
                encoder.SetSoftwareBitmap(softBitmap);
                await encoder.FlushAsync();

                ImageSaved?.Invoke(file);
                saveButton.Content = "Saved";
                saveButton.Visibility = Visibility.Collapsed;

                var savedText = new TextBlock
                {
                    Text = $"Saved as {fileName}",
                    FontSize = 11,
                    Margin = new Thickness(0, 8, 0, 0),
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                };
                ((StackPanel)saveButton.Parent).Children.Add(savedText);
            }
            catch (Exception ex)
            {
                ShowError($"Save failed: {ex.Message}");
                saveButton.IsEnabled = true;
            }
        };

        var stack = new StackPanel();
        stack.Children.Add(image);
        stack.Children.Add(saveButton);

        return new Border
        {
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = (Brush)Application.Current.Resources["AppAccentBrush"],
            Padding = new Thickness(8),
            Child = stack
        };
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
