using NodaTime;
using System;

namespace ArchivePlanner.Planning.Model
{
    public abstract class BackupSchedule : ICloneable
    {
        public BackupSchedule()
        {
        }

        public BackupSchedule(LocalTime executionTime)
        {
            ExecutionTime = executionTime;
        }

        public BackupSchedule(BackupSchedule schedule)
        {
            ExecutionTime = schedule.ExecutionTime;
        }

        public LocalTime ExecutionTime { get; set; } = LocalTime.Midnight;

        public LocalDateTime? LastExecution { get; set; }


        public abstract LocalDateTime NextExecution(LocalDateTime now);

        public abstract object Clone();
    }
}
