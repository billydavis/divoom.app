using divoom.app.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using WinRT.Interop;

namespace divoom.app;

public sealed partial class ImageViewerPage : Page, INotifyPropertyChanged
{
    public ObservableCollection<ImageFileInfo> Images { get; } = new();

    private ImageFileInfo? _selectedImage;
    private ImageFileInfo? _contextMenuTarget;

    private bool _isLoading;
    public bool IsNotLoading => !_isLoading;
    public Visibility LoadButtonTextVisibility => _isLoading ? Visibility.Collapsed : Visibility.Visible;
    public Visibility LoadingIndicatorVisibility => _isLoading ? Visibility.Visible : Visibility.Collapsed;
    public Visibility LoadingPlaceholderVisibility => _isLoading && Images.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility EmptyStateVisibility => !_isLoading && Images.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ImageListVisibility => Images.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null!) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void NotifyLoadingChanged()
    {
        OnPropertyChanged(nameof(IsNotLoading));
        OnPropertyChanged(nameof(LoadButtonTextVisibility));
        OnPropertyChanged(nameof(LoadingIndicatorVisibility));
        OnPropertyChanged(nameof(LoadingPlaceholderVisibility));
        OnPropertyChanged(nameof(EmptyStateVisibility));
        OnPropertyChanged(nameof(ImageListVisibility));
    }

    public ImageViewerPage()
    {
        InitializeComponent();
        Images.CollectionChanged += (_, _) => NotifyLoadingChanged();
        InitializeAsync();
        CreateImagePage.ImageSaved += OnImageSaved;
        AppSettings.FoldersChanged += OnFoldersChanged;
    }

    private async void OnFoldersChanged() => await ReloadAsync();

    private async void OnImageSaved(StorageFile file)
    {
        Images.Add(await LoadImageInfoAsync(file));
    }

    private async void InitializeAsync() => await ReloadAsync();

    private async Task ReloadAsync()
    {
        if (_isLoading) return;
        _isLoading = true;
        NotifyLoadingChanged();
        Images.Clear();
        _selectedImage = null;
        await GetItemsAsync();
        _isLoading = false;
        NotifyLoadingChanged();
    }

    private static async Task<StorageFolder> GetDefaultStorageFolderAsync()
    {
        var token = AppSettings.DefaultStorageFolderToken;
        if (!string.IsNullOrEmpty(token))
        {
            try { return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token); }
            catch { }
        }
        var path = System.IO.Path.Combine(
            Windows.Storage.UserDataPaths.GetDefault().LocalAppData,
            "DivoomManager", "Images");
        System.IO.Directory.CreateDirectory(path);
        return await StorageFolder.GetFolderFromPathAsync(path);
    }

    private async Task GetItemsAsync()
    {
        try
        {
            var defaultFolder = await GetDefaultStorageFolderAsync();
            foreach (StorageFile file in await defaultFolder.CreateFileQueryWithOptions(new QueryOptions()).GetFilesAsync())
                Images.Add(await LoadImageInfoAsync(file));
        }
        catch (Exception) { }

        foreach (var token in AppSettings.ExtraFolderTokens)
        {
            try
            {
                var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                foreach (StorageFile file in await folder.CreateFileQueryWithOptions(new QueryOptions()).GetFilesAsync())
                    Images.Add(await LoadImageInfoAsync(file));
            }
            catch (Exception) { }
        }
    }

    private async void ReloadButton_Click(object sender, RoutedEventArgs e) => await ReloadAsync();

    private void ImageRepeater_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var element = e.OriginalSource as UIElement;
        while (element is not null && element != ImageRepeater)
        {
            int index = ImageRepeater.GetElementIndex(element);
            if (index >= 0)
            {
                var tapped = Images[index];
                if (_selectedImage == tapped)
                {
                    tapped.IsSelected = false;
                    _selectedImage = null;
                }
                else
                {
                    if (_selectedImage is not null)
                        _selectedImage.IsSelected = false;
                    tapped.IsSelected = true;
                    _selectedImage = tapped;
                }
                return;
            }
            element = VisualTreeHelper.GetParent(element) as UIElement;
        }
    }

    private void ImageRepeater_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var element = e.OriginalSource as UIElement;
        while (element is not null && element != ImageRepeater)
        {
            int index = ImageRepeater.GetElementIndex(element);
            if (index >= 0)
            {
                _contextMenuTarget = Images[index];
                ImageContextMenu.ShowAt(element as FrameworkElement, e.GetPosition(element));
                return;
            }
            element = VisualTreeHelper.GetParent(element) as UIElement;
        }
    }

    private async void SendToDevice_Click(object sender, RoutedEventArgs e)
    {
        var image = _contextMenuTarget;
        var device = AppState.SelectedDevice;

        if (image is null) return;

        if (device is null || string.IsNullOrEmpty(device.IpAddress))
        {
            ShowFeedback(Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning,
                "No device selected — pick one from the Devices page first.");
            return;
        }

        var targetSize = 64; // all current devices use 64x64 panels

        _isLoading = true;
        NotifyLoadingChanged();
        try
        {
            var isTimesGate = device.Hardware == 400;
            var imageData = isTimesGate
                ? await ImageResizeCache.GetOrCacheJpegAsync(image.ImageFile, targetSize)
                : await ImageResizeCache.GetOrCacheRgbAsync(image.ImageFile, targetSize);
            var divoomDevice = new Divoom.Device(device.IpAddress);
            var lcdIndex = isTimesGate ? device.SelectedChannel : -1;
            var success = await divoomDevice.SendImageAsync(targetSize, imageData, lcdIndex);

            ShowFeedback(
                success ? Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success
                        : Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error,
                success ? $"Sent to {device.Name}."
                        : "Device did not accept the image — it may be offline.");
        }
        catch (Exception ex)
        {
            ShowFeedback(Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error, ex.Message);
        }
        finally
        {
            _isLoading = false;
            NotifyLoadingChanged();
        }
    }

    private async void ShowFeedback(Microsoft.UI.Xaml.Controls.InfoBarSeverity severity, string message)
    {
        SendFeedbackBar.Severity = severity;
        SendFeedbackBar.Message = message;
        SendFeedbackBar.IsOpen = true;
        await Task.Delay(3000);
        SendFeedbackBar.IsOpen = false;
    }

    private async void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".gif");
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

        var files = await picker.PickMultipleFilesAsync();
        if (files is null || files.Count == 0) return;

        var errors = new List<string>();
        var saveFolder = await GetDefaultStorageFolderAsync();

        foreach (var file in files)
        {
            try
            {
                using var stream = await file.OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(stream);
                uint w = decoder.PixelWidth, h = decoder.PixelHeight;

                if (w != h)
                {
                    errors.Add($"{file.Name}: must be square (is {w}×{h}).");
                    continue;
                }
                if (w > 1024)
                {
                    errors.Add($"{file.Name}: too large (max 1024×1024, is {w}×{h}).");
                    continue;
                }

                await file.CopyAsync(saveFolder, file.Name, NameCollisionOption.GenerateUniqueName);
            }
            catch (Exception ex)
            {
                errors.Add($"{file.Name}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Some files were skipped",
                Content = string.Join("\n", errors),
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await errorDialog.ShowAsync();
        }

        await ReloadAsync();
    }

    private async void DeleteImage_Click(object sender, RoutedEventArgs e)
    {
        if (_contextMenuTarget is null) return;

        var dialog = new ContentDialog
        {
            Title = "Delete image?",
            Content = $"\"{_contextMenuTarget.ImageName}\" will be permanently deleted.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        try
        {
            await _contextMenuTarget.ImageFile.DeleteAsync();
            if (_selectedImage == _contextMenuTarget)
                _selectedImage = null;
            Images.Remove(_contextMenuTarget);
        }
        catch (Exception) { }
    }

    public static async Task<ImageFileInfo> LoadImageInfoAsync(StorageFile file)
    {
        var properties = await file.Properties.GetImagePropertiesAsync();
        var info = new ImageFileInfo(properties, file, file.DisplayName, file.DisplayType);
        info.Source = await info.GetImageSourceAsync(128);
        return info;
    }
}
