using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WildWildWifi.GUI
{
    [ValueConversion(typeof(object), typeof(string))]
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            double seconds = (double)value;
            return ((int)seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return "";
            // Do the conversion from visibility to bool
        }
    }
}
