using ArchivePlanner.Util;
using System;
using System.Globalization;

namespace ArchivePlanner.Planning.Converters
{
    public class ScheduleToBooleanConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Type)parameter).IsAssignableFrom(value.GetType());
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override BaseConverter CreateConverter()
        {
            return new ScheduleToBooleanConverter();
        }
    }
}
