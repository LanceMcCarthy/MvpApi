using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MvpApi.Wpf.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Value Converter that can be used for empty strings as well as objects
    /// </summary>
    internal class NullToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if(IsInverted)
                    return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;

                return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                if(IsInverted)
                    return value == null ? Visibility.Visible : Visibility.Collapsed;

                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
