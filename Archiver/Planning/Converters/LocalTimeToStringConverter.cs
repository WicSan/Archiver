using Archiver.Util;
using NodaTime;
using System;
using System.Globalization;

namespace Archiver.Planning.Converters
{
    public class LocalDateTimeToStringConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((LocalDateTime?)value)?.ToString("ddd, dd.MM.yyyy HH:mm", null) ?? "-";
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
