using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ArchivePlanner.Util
{
    public abstract class BaseConverter : MarkupExtension, IValueConverter
    {
        private BaseConverter? _converter;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ??= CreateConverter();
        }

        protected abstract BaseConverter CreateConverter();
    }
}
