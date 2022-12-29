using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace Archiver.Planning.Model
{
    public class WeeklyBackupSchedule : BackupSchedule
    {
        public HashSet<IsoDayOfWeek> ExecutionDays { get; set; } = new HashSet<IsoDayOfWeek>();

        public WeeklyBackupSchedule() : base(LocalTime.Noon)
        {
        }

        public WeeklyBackupSchedule(LocalTime executionTime, IEnumerable<IsoDayOfWeek> executionDays) : base(executionTime)
        {
            ExecutionDays = executionDays.Distinct().ToHashSet();
        }

        public WeeklyBackupSchedule(BackupSchedule schedule) : base(schedule)
        {
            if(schedule is WeeklyBackupSchedule weekly)
                ExecutionDays = weekly.ExecutionDays;
        }

        public override LocalDateTime NextExecution(LocalDateTime now)
        {
            // use yesterday so that the current today is still a viable option
            var yesterday = now.Minus(Period.FromDays(1));
            var nextExecution = ExecutionDays
                .Select(day => (LastExecution ?? yesterday).Date.Next(day))
                .OrderBy(date => date)
                .First();

            return nextExecution.At(ExecutionTime);
        }

        public override object Clone()
        {
            return new WeeklyBackupSchedule(this);
        }
    }
}
