using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace TestWpfApplication
{
    public class DoubleToStringConverter : IValueConverter
    {
        private int _digits;

        // Convert from double to string
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string str = value.ToString();

            if (str.Length < _digits)
            {
                if (!ContainDecimalSeparator(str))
                            str = str + ',';
                while (str.Length < _digits)
                {
                    str = str + '0';
                }
            }
            return str;
        }

        // Convert from string to double
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var pureStr = CleanString(value.ToString());
            _digits = pureStr.Length;

            double? result = null;
            try
            {
                result = System.Convert.ToDouble(pureStr,CultureInfo.CurrentUICulture);
            }
            catch
            {
            }

            return result.HasValue ? (object)result.Value : DependencyProperty.UnsetValue;
        }

        private string CleanString(string str)
        {
            if (str == "0") return str;
            var pureStr = "";
            // "-" is allowed only as a first symbol
            if (str.StartsWith("-")) pureStr = "-";

            foreach (var ch in str)
            {
                // leading zeros are not allowed
                if ((ch == '0') && ((pureStr == "") || (pureStr == "-"))) continue;
                // another digits are allowed in any place
                if (char.IsDigit(ch)) pureStr = pureStr + ch;

                if ((ch == '.' || ch == ',') && !ContainDecimalSeparator(pureStr))
                {
                    if ((pureStr == "") || (pureStr == "-")) pureStr = pureStr + "0";
                    pureStr = pureStr + CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
                }
            }
            return pureStr;
        }

        private bool ContainDecimalSeparator(string str)
        {
            return str.Contains(',') || str.Contains('.');
        }
    }
}
