using NodaTime;
using NodaTime.Text;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ArchivePlanner.Util
{
    public class LocalTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return ((LocalTime)value).ToString("HH:mm", null);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            var pattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");
            var parsedValue = pattern.Parse((string)value);
            return parsedValue.Success ? parsedValue.Value : LocalTime.Midnight;
        }
    }
}
