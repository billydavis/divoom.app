using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.Storage;

namespace divoom.app;

public class ImageFileInfo : INotifyPropertyChanged
{
    public ImageFileInfo(ImageProperties properties,
        StorageFile imageFile,
        string name,
        string type)
    {
        ImageProperties = properties;
        ImageName = name;
        ImageFileType = type;
        ImageFile = imageFile;
        var rating = (int)properties.Rating;
        var random = new Random();
        ImageRating = rating == 0 ? random.Next(1, 5) : rating;
    }

    public StorageFile ImageFile { get; }

    public ImageProperties ImageProperties { get; }

    public async Task<BitmapImage> GetImageSourceAsync(int decodePixelWidth = 0)
    {
        using IRandomAccessStream fileStream = await ImageFile.OpenReadAsync();
        var bitmapImage = new BitmapImage();
        if (decodePixelWidth > 0)
            bitmapImage.DecodePixelWidth = decodePixelWidth;
        await bitmapImage.SetSourceAsync(fileStream);
        return bitmapImage;
    }

    public async Task<BitmapImage> GetImageThumbnailAsync()
    {
        StorageItemThumbnail thumbnail =
            await ImageFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 128);
        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(thumbnail);
        thumbnail.Dispose();
        return bitmapImage;
    }

    public string ImageName { get; }

    public string ImageFileType { get; }

    public string ImageDimensions => $"{ImageProperties.Width} x {ImageProperties.Height}";

    public string ImageTitle
    {
        get => string.IsNullOrEmpty(ImageProperties.Title) ? ImageName : ImageProperties.Title;
        set
        {
            if (ImageProperties.Title != value)
            {
                ImageProperties.Title = value;
                _ = ImageProperties.SavePropertiesAsync();
                OnPropertyChanged();
            }
        }
    }

    public int ImageRating
    {
        get => (int)ImageProperties.Rating;
        set
        {
            if (ImageProperties.Rating != value)
            {
                ImageProperties.Rating = (uint)value;
                _ = ImageProperties.SavePropertiesAsync();
                OnPropertyChanged();
            }
        }
    }

    private BitmapImage _source;
    public BitmapImage Source
    {
        get => _source;
        set { _source = value; OnPropertyChanged(); }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectionBorderThickness));
            }
        }
    }

    public Microsoft.UI.Xaml.Thickness SelectionBorderThickness =>
        IsSelected ? new Microsoft.UI.Xaml.Thickness(3) : new Microsoft.UI.Xaml.Thickness(0);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}