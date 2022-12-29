using Archiver.Util;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Effects;

namespace Archiver.Planning.Converters
{
    public class ProgressToBlurConverter : BaseConverter
    {
        protected override BaseConverter CreateConverter()
        {
            return new ProgressToBlurConverter();
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = new ProgressToBooleanConverter();
            return (bool)converter.Convert(value, targetType, parameter, culture) ? new BlurEffect() : DependencyProperty.UnsetValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
