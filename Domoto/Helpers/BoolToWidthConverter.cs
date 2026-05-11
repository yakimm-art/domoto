using System;
using System.Globalization;
using System.Windows.Data;

namespace Domoto.Helpers
{
    public class BoolToWidthConverter : IValueConverter
    {
        public double ExpandedWidth { get; set; } = 260;
        public double CollapsedWidth { get; set; } = 70;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? CollapsedWidth : ExpandedWidth;
            }
            return ExpandedWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
