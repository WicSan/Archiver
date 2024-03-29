﻿using Archiver.Backup;
using Archiver.Backup.Model;
using Archiver.Util;
using System;
using System.Globalization;

namespace Archiver.Planning.Converters
{
    public class ProgressToBooleanConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(typeof(Conditional), (string?)parameter, out var conditionalType);
            var isInverted = (Conditional?)conditionalType == Conditional.Inverted;
            var inProgress = (BackupProgress)value is not null && ((BackupProgress)value).Percentage != ProgressService.TaskCompleted;
            return isInverted != inProgress;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override BaseConverter CreateConverter()
        {
            return new ProgressToBooleanConverter();
        }
    }
}
