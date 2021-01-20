using System;
using System.Globalization;
using System.Windows.Data;

namespace MvpApi.Wpf.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Returns a chosen IconElement depending on the boolean value
    /// </summary>
    //internal class BoolToIconConverter : IValueConverter
    //{
    //    public IconElement TrueIcon { get; set; }
    //    public IconElement FalseIcon { get; set; }

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is bool val)
    //        {
    //            return val ? TrueIcon : FalseIcon;
    //        }

    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is IconElement icon)
    //        {
    //            return icon == TrueIcon;
    //        }

    //        return null;
    //    }
    //}
}
