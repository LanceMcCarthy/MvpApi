using MvpApi.Common.Models;
using System.Globalization;

namespace MvpCompanion.Maui.Converters;

internal class PrivacyLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = Colors.LightGray;

        if (value is VisibilityViewModel privacyLevel)
        {
            if (privacyLevel.Id == 299600000 || privacyLevel.Description == "Microsoft") // Microsoft
            {
                color = Color.FromRgba(0xF6, 0x37, 0x37, 0xFF); //Red
            }
            else if (privacyLevel.Id == 299600003 || privacyLevel.Description == "MVP Community") // Other MVPs
            {
                color = Colors.Goldenrod;
            }
            else if (privacyLevel.Id == 299600002 || privacyLevel.Description == "Everyone") //everyone
            {
                color = Colors.Green;
            }
        }

        return color;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
