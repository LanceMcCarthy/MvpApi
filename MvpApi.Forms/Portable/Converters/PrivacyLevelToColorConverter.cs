using System;
using System.Globalization;
using MvpApi.Common.Models;
using Xamarin.Forms;

namespace MvpApi.Forms.Portable.Converters
{
    internal class PrivacyLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Color.LightGray;

            if (value is VisibilityViewModel privacyLevel)
            {
                if (privacyLevel.Id == 299600000 || privacyLevel.Description == "Microsoft") // Microsoft
                {
                    color = Color.FromRgba(0xF6, 0x37, 0x37, 0xFF); //Red
                }
                else if (privacyLevel.Id == 299600003 || privacyLevel.Description == "MVP Community") // Other MVPs
                {
                    color = Color.Goldenrod;
                }
                else if (privacyLevel.Id == 299600002 || privacyLevel.Description == "Everyone") //everyone
                {
                    color = Color.Green;
                }
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
