using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using MvpApi.Common.Models;

namespace MvpCompanion.UI.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Converts UploadStatus enumeration to a color (a visual indicator for upload state).
    /// </summary>
    internal class UploadStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UploadStatus status)
            {
                switch (status)
                {
                    case UploadStatus.Pending:
                        return new SolidColorBrush(Colors.LightGray);
                    case UploadStatus.InProgress:
                        return new SolidColorBrush(Colors.Gold);
                    case UploadStatus.Success:
                        return new SolidColorBrush(Colors.LightGreen);
                    case UploadStatus.Failed:
                        return new SolidColorBrush(Color.FromArgb(0xFF,0xF6,0x37,0x37));
                    case UploadStatus.None:
                        return new SolidColorBrush(Colors.Transparent);
                }
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
