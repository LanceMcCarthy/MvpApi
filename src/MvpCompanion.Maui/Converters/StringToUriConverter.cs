using System.Globalization;

namespace MvpCompanion.Maui.Converters
{
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is Uri uri)
            {
                return uri.AbsoluteUri;
            }

            return "";
        }
    }
}
