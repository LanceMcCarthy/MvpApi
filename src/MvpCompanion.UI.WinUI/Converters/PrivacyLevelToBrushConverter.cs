using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MvpApi.Common.Models;
using Windows.UI;

namespace MvpCompanion.UI.WinUI.Converters;

public class PrivacyLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var color = Colors.LightGray;

        if (value is VisibilityViewModel privacyLevel)
        {
            if (privacyLevel.Id == 299600000 || privacyLevel.Description == "Microsoft") // Microsoft
            {
                color = Color.FromArgb(0xFF, 0xF6, 0x37, 0x37);
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

        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}