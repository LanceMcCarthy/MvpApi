using System.Globalization;

namespace MvpCompanion.Maui.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is string s)
            {
                if (IsInverted)
                    return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;

                return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                if (IsInverted)
                    return value == null ? Visibility.Visible : Visibility.Collapsed;

                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
