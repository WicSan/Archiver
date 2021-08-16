using NodaTime;
using System;

namespace FastBackup.Planning.Model
{
    public class WeeklyBackupPlan : BackupPlan
    {
        public LocalTime ExecutionStart { get; set; }

        public DayOfWeek[] Interval { get; set; } = Array.Empty<DayOfWeek>();
    }
}
