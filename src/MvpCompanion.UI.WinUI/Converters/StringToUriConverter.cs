using System;
using Windows.UI.Xaml.Data;

namespace MvpCompanion.UI.WinUI.Converters
{
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Uri uri)
            {
                return uri.AbsoluteUri;
            }

            return "";
        }
    }
}
