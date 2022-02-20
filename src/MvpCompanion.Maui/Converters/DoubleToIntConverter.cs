using System.Globalization;

namespace MvpCompanion.Maui.Converters
{
    public class DoubleToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
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
