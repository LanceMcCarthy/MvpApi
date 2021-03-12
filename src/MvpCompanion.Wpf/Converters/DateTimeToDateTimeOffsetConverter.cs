using System;
using System.Globalization;
using System.Windows.Data;

namespace MvpCompanion.Wpf.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Converts between DateTime and DateTimeOffset
    /// </summary>
    internal class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var date = (DateTime)value;
                return new DateTimeOffset(date);
            }
            catch
            {
                return DateTimeOffset.MinValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var dto = (DateTimeOffset)value;
                return dto.DateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
