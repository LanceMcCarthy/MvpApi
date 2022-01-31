using System;
using Microsoft.UI.Xaml.Data;

namespace MvpCompanion.UI.WinUI.Converters;

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