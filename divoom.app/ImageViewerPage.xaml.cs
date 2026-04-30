using divoom.app.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Search;

namespace divoom.app;

public sealed partial class ImageViewerPage : Page
{
    public ObservableCollection<ImageFileInfo> Images { get; } = new();

    private ImageFileInfo? _selectedImage;

    public ImageViewerPage()
    {
        InitializeComponent();
        InitializeAsync();
        CreateImagePage.ImageSaved += OnImageSaved;
        AppState.SelectionChanged += OnSelectionChanged;
        UpdateTargetBar();
    }

    private void OnSelectionChanged()
    {
        DispatcherQueue.TryEnqueue(UpdateTargetBar);
    }

    private void UpdateTargetBar()
    {
        var dev = AppState.SelectedDevice;
        if (dev is null)
        {
            SendTargetText.Text = "No target selected — go to Devices to select one";
            return;
        }
        SendTargetText.Text = dev.HasMultipleChannels
            ? $"{dev.Name}  ·  Screen {AppState.EffectiveChannel + 1}"
            : dev.Name;
    }

    private async void OnImageSaved(StorageFile file)
    {
        Images.Add(await LoadImageInfoAsync(file));
    }

    private async void InitializeAsync() => await GetItemsAsync();

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
            element = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(element) as UIElement;
        }
    }

    public static async Task<ImageFileInfo> LoadImageInfoAsync(StorageFile file)
    {
        var properties = await file.Properties.GetImagePropertiesAsync();
        var info = new ImageFileInfo(properties, file, file.DisplayName, file.DisplayType);
        info.Source = await info.GetImageSourceAsync();
        return info;
    }
}
