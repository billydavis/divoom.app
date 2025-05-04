using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace divoom.app
{
    public class HardwareToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int hardware)
            {
                switch (hardware)
                {
                    case 400:
                        return new BitmapImage(new Uri("ms-appx:///Assets/timesGate.png"));
                    case 92:
                        return new BitmapImage(new Uri("ms-appx:///Assets/pixoo64.png"));
                    // Add more cases as needed
                    default:
                        return new BitmapImage(new Uri("ms-appx:///Assets/storeLogo.png"));
                }
            }
            return new BitmapImage(new Uri("ms-appx:///Assets/storeLogo.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

