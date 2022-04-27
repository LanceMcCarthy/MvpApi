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
            case "Upload":
                return (target == "Upload") ? "tab_upload_on.png" : "tab_upload.png";
            case "Account":
                return (target == "Account") ? "tab_briefcase_on.png" : "tab_briefcase.png";
            case "Settings":
                return (target == "Settings") ? "tab_settings_on.png" : "tab_settings.png";
            default:
                return (target == "Settings") ? "tab_settings_on.png" : "tab_settings.png";
        }
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (string)value;
    }
}
