using System;
using System.Globalization;
using Xamarin.Forms;

namespace MvpApi.Forms.Portable.Converters
{
    internal class MvcContentUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string mvcUrl)
            {
                return mvcUrl.Replace("~", "https://mvp.microsoft.com");
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string webUrl)
            {
                return webUrl.Replace("https://mvp.microsoft.com", "~");
            }

            return "";
        }
    }
}
