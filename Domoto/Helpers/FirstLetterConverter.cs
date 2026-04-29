using System;
using System.Globalization;
using System.Windows.Data;

namespace Domoto.Helpers
{
    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (!string.IsNullOrEmpty(str))
                return str.Substring(0, 1).ToUpper();
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
