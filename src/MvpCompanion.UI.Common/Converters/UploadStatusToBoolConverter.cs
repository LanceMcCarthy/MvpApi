using System;
using Windows.UI.Xaml.Data;
using MvpApi.Common.Models;

namespace MvpCompanion.UI.Common.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Returns error color if upload failed
    /// </summary>
    public class UploadStatusToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UploadStatus status)
            {
                return status == UploadStatus.Failed;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool val && val == true)
            {
                return UploadStatus.Failed;
            }

            return default(UploadStatus);
        }
    }
}
