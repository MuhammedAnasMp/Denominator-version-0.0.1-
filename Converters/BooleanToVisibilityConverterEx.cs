using System;
using System.Globalization;
using System.Windows.Data;

namespace Deno.Converters
{
    public class BooleanToButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool updatingRecord && updatingRecord) ? "Update" : "Submit";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}