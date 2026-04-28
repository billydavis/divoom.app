using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
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
    }

    private async void InitializeAsync() => await GetItemsAsync();

    private async Task GetItemsAsync()
    {
        StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
        StorageFolder picturesFolder = await appInstalledFolder.GetFolderAsync("Images");

        var result = picturesFolder.CreateFileQueryWithOptions(new QueryOptions());
        IReadOnlyList<StorageFile> imageFiles = await result.GetFilesAsync();
        foreach (StorageFile file in imageFiles)
            Images.Add(await LoadImageInfoAsync(file));
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
