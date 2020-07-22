using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MvpApi.Common.Models;

namespace MvpApi.Wpf.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Converts UploadStatus enumeration to a color (a visual indicator for upload state).
    /// </summary>
    internal class UploadStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
