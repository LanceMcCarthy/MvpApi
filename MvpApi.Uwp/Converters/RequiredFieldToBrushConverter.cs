using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MvpApi.Uwp.Converters
{
    internal class RequiredFieldToBrushConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush brush = null;

            try
            {
                if (value is bool isRequired)
                {
                    if (IsInverted)
                    {
                        brush = isRequired
                            ? App.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush
                            : App.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
                    }
                    else
                    {
                        brush = isRequired
                            ? App.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush
                            : App.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
                    }
                }
            }
            catch
            {
                brush = new SolidColorBrush(Colors.Black);
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
