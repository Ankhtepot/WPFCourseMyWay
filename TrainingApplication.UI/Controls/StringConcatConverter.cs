using System;
using System.Globalization;
using System.Windows.Data;

namespace TrainingApplication.UI.Controls
{
    public class StringConcatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string separator = parameter as string ?? " ";

            return string.Join(separator, values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string separator = parameter as string ?? " ";
            return ((string) value).Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}