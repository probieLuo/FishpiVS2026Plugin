using System;
using System.Globalization;
using System.Windows.Data;

namespace FishpiVS2026Plugin.Converters
{
    internal class BreezemoonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string htmlstr)
            {
                int startIndex = htmlstr.IndexOf("<p>", StringComparison.OrdinalIgnoreCase);
                int endIndex = htmlstr.IndexOf("</p>", StringComparison.OrdinalIgnoreCase);

                if (startIndex == -1 || endIndex == -1 || startIndex >= endIndex)
                    return htmlstr;

                startIndex += 3;
                string innerText = htmlstr.Substring(startIndex, endIndex - startIndex);
                return innerText;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
