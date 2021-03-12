using System;
using System.Globalization;
using System.Windows.Data;
using MvpApi.Common.Models;

namespace MvpCompanion.Wpf.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Returns error color if upload failed
    /// </summary>
    internal class UploadStatusToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UploadStatus status)
            {
                return status == UploadStatus.Failed;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val && val == true)
            {
                return UploadStatus.Failed;
            }

            return default(UploadStatus);
        }
    }
}
