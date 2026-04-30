using Microsoft.UI.Xaml.Data;
using System;

namespace divoom.app
{
    public class HardwareToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int hardware)
            {
                return hardware switch
                {
                    400 => "Times Gate",
                    92  => "Pixoo 64",
                    _   => $"Device ({hardware})"
                };
            }
            return "Unknown Device";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
