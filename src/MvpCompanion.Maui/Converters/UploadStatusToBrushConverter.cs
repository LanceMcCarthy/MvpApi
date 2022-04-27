using MvpApi.Common.Models;
using System.Globalization;

namespace MvpCompanion.Maui.Converters
{
    public class UploadStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is UploadStatus status)
            {
                switch (status)
                {
                    case UploadStatus.Pending:
                        return Colors.LightGray;
                    case UploadStatus.InProgress:
                        return Colors.Gold;
                    case UploadStatus.Success:
                        return Colors.LightGreen;
                    case UploadStatus.Failed:
                        return Color.FromArgb("FFF63737");
                    case UploadStatus.None:
                        return new SolidColorBrush(Colors.Transparent);
                }
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
