using System.Globalization;

namespace MvpCompanion.Maui.Converters
{
    public class ThresholdConverter : IValueConverter
    {
        /// <summary>
        /// OverResult will be returned when the value is larger than the threshold amount (UnderResult is used when Threshold is equal to value)
        /// </summary>
        public int Threshold { get; set; }

        public object UnderResult { get; set; }

        public object OverResult { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is int val)
            {
                return val <= Threshold ? UnderResult : OverResult;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
