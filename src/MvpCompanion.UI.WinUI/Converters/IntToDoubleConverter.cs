using System;
using Microsoft.UI.Xaml.Data;

namespace MvpCompanion.UI.WinUI.Converters;

internal class IntToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return System.Convert.ToDouble(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return System.Convert.ToInt32(value);
    }
}