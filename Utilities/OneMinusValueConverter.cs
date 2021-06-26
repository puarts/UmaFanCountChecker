using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace UmaFanCountChecker
{
    public class OneMinusValueConverter : IValueConverter
    {
        public static OneMinusValueConverter Instance { get; } = new OneMinusValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float floatValue = (float)value;
            return 1.0f - floatValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
