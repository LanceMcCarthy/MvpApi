using System;
using Windows.UI.Xaml.Data;

namespace MvpApi.Uwp.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Converts between Double and Int32
    /// </summary>
    public class DoubleToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double source)
            {
                return System.Convert.ToInt32(source);
            }
            else
            {
                return value;
            }
        }
    }
}
