using System;
using Microsoft.UI.Xaml.Data;

namespace MvpCompanion.UI.WinUI.Converters;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
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