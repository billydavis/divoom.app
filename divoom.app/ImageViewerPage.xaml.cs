using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Search;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace divoom.app;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ImageViewerPage : Page
{
    public ObservableCollection<ImageFileInfo> Images { get; } =
        new ObservableCollection<ImageFileInfo>();

    public ImageViewerPage()
    {
        this.InitializeComponent();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await GetItemsAsync();
    }

    private async Task GetItemsAsync()
    {
        StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
        StorageFolder picturesFolder = await appInstalledFolder.GetFolderAsync("Images");

        var result = picturesFolder.CreateFileQueryWithOptions(new QueryOptions());

        IReadOnlyList<StorageFile> imageFiles = await result.GetFilesAsync();
        foreach (StorageFile file in imageFiles)
        {
            Images.Add(await LoadImageInfoAsync(file));
        }

        // ImageGridView.ItemsSource = Images;
    }

    public static async Task<ImageFileInfo> LoadImageInfoAsync(StorageFile file)
    {
        var properties = await file.Properties.GetImagePropertiesAsync();
        ImageFileInfo info = new(properties,
            file, file.DisplayName, file.DisplayType);

        return info;
    }

    private void ImageGridView_ContainerContentChanging(
        ListViewBase sender,
        ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
            var image = templateRoot.FindName("ItemImage") as Image;
            image.Source = null;
        }

        if (args.Phase == 0)
        {
            args.RegisterUpdateCallback(ShowImage);
            args.Handled = true;
        }
    }

    private async void ShowImage(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Phase == 1)
        {
            // It's phase 1, so show this item's image.
            var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
            var image = templateRoot.FindName("ItemImage") as Image;
            var item = args.Item as ImageFileInfo;
            image.Source = await item.GetImageThumbnailAsync();
        }
    }
}