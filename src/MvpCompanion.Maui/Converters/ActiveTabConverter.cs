using System.Globalization;

namespace MvpCompanion.Maui.Converters;

public class ActiveTabConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var target = (string)value;
        var tab = (string)parameter;

        switch (tab)
        {
            case "Home":
                return (target == "Home") ? "tab_home_on.png" : "tab_home.png";
            case "Profile":
                return (target == "Profile") ? "tab_map_on.png" : "tab_map.png";
            case "About":
                return (target == "About") ? "tab_favorites_on.png" : "tab_favorites.png";
            case "Settings":
            default:
                return (target == "Settings") ? "tab_settings_on.png" : "tab_settings.png";
        }
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (string)value;
    }
}
