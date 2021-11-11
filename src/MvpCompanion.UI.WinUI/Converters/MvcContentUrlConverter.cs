using System;
using Windows.UI.Xaml.Data;

namespace MvpCompanion.UI.WinUI.Converters
{
    /// <summary>
    /// // Because the IconURls are using MVC content path, we need to prefix with the domain to get valid images. This converter does that automatically.
    /// </summary>
    public class MvcContentUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string mvcUrl)
            {
                return mvcUrl.Replace("~", "https://mvp.microsoft.com");
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string webUrl)
            {
                return webUrl.Replace("https://mvp.microsoft.com", "~");
            }

            return "";
        }
    }
}
