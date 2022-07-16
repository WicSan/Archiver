using ArchivePlanner.Util;
using NodaTime;
using System;
using System.Globalization;

namespace ArchivePlanner.Planning.Converters
{
    public class LocalDateTimeToStringConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((LocalDateTime?)value)?.ToString("dd.MM.yyyy", null) ?? "-";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override BaseConverter CreateConverter()
        {
            return new LocalDateTimeToStringConverter();
        }
    }
}
