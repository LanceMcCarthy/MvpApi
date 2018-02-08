using System;
using Windows.UI.Xaml.Data;

namespace MvpApi.Uwp.Converters
{
    internal class DoubleToIntConverter : IValueConverter
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
