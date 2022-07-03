using ArchivePlanner.Util;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArchivePlanner.Planning.Converters
{
    public class ProgressToVisibilityConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = new ProgressToBooleanConverter();
            return (bool)converter.Convert(value, targetType, parameter, culture) ? Visibility.Visible : Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override BaseConverter CreateConverter()
        {
            return new ProgressToVisibilityConverter();
        }
    }
}
