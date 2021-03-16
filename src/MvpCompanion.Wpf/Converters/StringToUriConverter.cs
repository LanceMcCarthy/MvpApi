using System;
using System.Globalization;
using System.Windows.Data;

namespace MvpCompanion.Wpf.Converters
{
    internal class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = new Uri("https://bing.com");

            try
            {
                if (value is string url)
                {
                    uri = new Uri(url);
                }
            }
            catch
            {
                // ignored
            }

            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Uri uri)
            {
                return uri.AbsoluteUri;
            }

            return "";
        }
    }
}
