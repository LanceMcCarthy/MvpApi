using System;
using System.Globalization;
using System.Windows.Data;

namespace MvpApi.Wpf.Converters
{
    /// <summary>
    /// Adopted from Template10's ValueWhenConverter for UWP
    /// https://github.com/Windows-XAML/Template10/
    /// </summary>
    internal class ValueWhenConverter : IValueConverter
    {
        public object Value { get; set; }
        public object Otherwise { get; set; }
        public object When { get; set; }
        public object OtherwiseValueBack { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (object.Equals(value, parameter ?? When))
                {
                    return Value;
                }

                return Otherwise;
            }
            catch
            {
                return Otherwise;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (OtherwiseValueBack == null)
            {
                throw new InvalidOperationException("Cannot ConvertBack if no OtherwiseValueBack is set!");
            }

            try
            {
                if (object.Equals(value, Value))
                {
                    return When;
                }

                return OtherwiseValueBack;
            }
            catch
            {
                return OtherwiseValueBack;
            }
        }
    }
}
