using System;
using System.Globalization;
using System.Windows.Data;

namespace MvpCompanion.Wpf.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Converts between Double and Int32
    /// </summary>
    internal class DoubleToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
