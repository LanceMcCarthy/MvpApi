﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MvpApi.Common.Models;

namespace MvpCompanion.Wpf.Converters
{
    internal class PrivacyLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
