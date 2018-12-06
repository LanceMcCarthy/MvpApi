using System;
using Windows.UI.Xaml.Data;

namespace MvpApi.Uwp.Converters
{
    internal class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string url)
            {
                return new Uri(url);
            }

            return new Uri("");
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
