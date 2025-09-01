using System;
using System.Globalization;
using System.Windows.Data;

namespace Deno.Converters
{
    public class ButtonContentMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return "Sent"; // default

            bool updating = values[0] is bool b1 && b1;
            bool editMod = values[1] is bool b2 && b2;

            if (editMod)
                return "Unlock 🔒";

            if (updating)
                return "Update";

            return "Sent";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
