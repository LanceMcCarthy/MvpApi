using System;
using Windows.UI.Xaml.Data;

namespace MvpApi.Uwp.Converters
{
    public class ThresholdConverter : IValueConverter
    {
        /// <summary>
        ///The value that the UnderResult will be returned for. Ex. If the threshold is 10, OverResult will be returned when Value hits 11
        /// </summary>
        public int Threshold { get; set; }

        public object UnderResult { get; set; }

        public object OverResult { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int val)
            {
                return val <= Threshold ? UnderResult : OverResult;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
