using System.Globalization;

namespace MvpCompanion.Maui.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        public string TrueIcon { get; set; }

        public string FalseIcon { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is bool val)
            {
                return val ? TrueIcon : FalseIcon;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is string icon)
            {
                return icon == TrueIcon;
            }

            return null;
        }
    }
}
