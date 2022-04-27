using System.Globalization;

namespace MvpCompanion.Maui.Converters;

public class ConfigurableBoolConverter : IValueConverter
{
    public ConfigurableBoolConverter() { }

    public ConfigurableBoolConverter(object trueResult, object falseResult)
    {
        this.TrueResult = trueResult;
        this.FalseResult = falseResult;
    }

    public object TrueResult { get; set; }

    public object FalseResult { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (this.TrueResult == null || this.FalseResult == null)
        {
            return !(bool)value;
        }

        return value is bool b && b ? this.TrueResult : this.FalseResult;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (this.TrueResult == null || this.FalseResult == null)
        {
            return !(bool)value;
        }

        return value is object variable && EqualityComparer<object>.Default.Equals(variable, this.TrueResult);
    }
}
