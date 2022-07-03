using NodaTime;

namespace ArchivePlanner.Planning.Model
{
    public class DailyBackupSchedule : BackupSchedule
    {
        public DailyBackupSchedule() : base(LocalTime.Noon)
        {
        }

        public DailyBackupSchedule(LocalTime executionTime) : base(executionTime)
        {
        }

        public DailyBackupSchedule(BackupSchedule schedule) : base(schedule)
        {
        }

        public override LocalDateTime NextExecution(LocalDateTime now)
        {
            if (LastExecution is null)
            {
                return now.Date.At(ExecutionTime);
            }

            return LastExecution!.Value.Date.PlusDays(1).At(ExecutionTime);
        }

        public override object Clone()
        {
            return new DailyBackupSchedule(this);
        }
    }
}
