using System;
using System.Globalization;
using System.Windows.Data;

namespace Domoto.Helpers
{
    public class CompletionVerbalizer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? "completed" : "not completed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}