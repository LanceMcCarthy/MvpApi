using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MvpApi.Uwp.Converters
{
    /// <summary>
    /// Value Converter that can be used for empty strings as well as objects
    /// </summary>
    class NullToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                if(IsInverted)
                    return string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;

                return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                if(IsInverted)
                    return value == null ? Visibility.Visible : Visibility.Collapsed;

                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
