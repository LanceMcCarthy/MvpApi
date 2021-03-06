﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace MvpCompanion.Wpf.Converters
{
    /// <summary>
    /// // Because the IconURls are using MVC content path, we need to prefix with the domain to get valid images. This converter does that automatically.
    /// </summary>
    internal class MvcContentUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string mvcUrl)
            {
                return  mvcUrl.Replace("~", "https://mvp.microsoft.com");
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string webUrl)
            {
                return webUrl.Replace("https://mvp.microsoft.com", "~");
            }

            return "";
        }
    }
}
