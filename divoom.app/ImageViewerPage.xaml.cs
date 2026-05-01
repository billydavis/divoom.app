using divoom.app.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

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
    }

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

    private async Task GetItemsAsync()
    {
        try
        {
            StorageFolder installedFolder = await Package.Current.InstalledLocation.GetFolderAsync("Images");
            foreach (StorageFile file in await installedFolder.CreateFileQueryWithOptions(new QueryOptions()).GetFilesAsync())
                Images.Add(await LoadImageInfoAsync(file));
        }
        catch (Exception) { }

        try
        {
            StorageFolder generatedFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Images");
            foreach (StorageFile file in await generatedFolder.CreateFileQueryWithOptions(new QueryOptions()).GetFilesAsync())
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

    private void SendToDevice_Click(object sender, RoutedEventArgs e)
    {
        // TODO: wire up to Divoom API
    }

    private async void DeleteImage_Click(object sender, RoutedEventArgs e)
    {
        if (_contextMenuTarget is null) return;

        var installedPath = Package.Current.InstalledLocation.Path;
        if (_contextMenuTarget.ImageFile.Path.StartsWith(installedPath, StringComparison.OrdinalIgnoreCase))
        {
            var info = new ContentDialog
            {
                Title = "Cannot delete",
                Content = "Built-in images cannot be deleted.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await info.ShowAsync();
            return;
        }

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
