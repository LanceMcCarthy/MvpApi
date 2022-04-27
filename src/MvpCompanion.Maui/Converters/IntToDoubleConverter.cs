using System.Globalization;

namespace MvpCompanion.Maui.Converters
{
    public class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return System.Convert.ToDouble(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return System.Convert.ToInt32(value);
        }
    }
}
