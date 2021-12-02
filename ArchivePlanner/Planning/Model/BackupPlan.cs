using ArchivePlanner.Util;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArchivePlanner.Planning.Model
{
    public abstract class BackupPlan
    {
        public string Name { get; set; } = null!;

        public DirectoryInfo Destination { get; set; } = null!;

        public ICollection<FileSystemInfo> FileSystemItems { get; set; } = new List<FileSystemInfo>();

        public ZonedDateTime? LastExecution { get; set; }

        public IsoDayOfWeek[] ExecutionDays { get; set; } = Array.Empty<IsoDayOfWeek>();

        public LocalTime ExecutionTime { get; set; }

        public string UniqueName
        {
            get
            {
                var timestamp = SystemClock.Instance.GetCurrentInstant().ToString("yyyy-MM-dd-HH-mm-ss", null);
                return $"{Name}_{timestamp}";
            }
        }

        public abstract IEnumerable<FileInfo> GetFilesToBackup();

        public ZonedDateTime? CalculateNextExecution(ZonedDateTime now)
        {
            var nextExecution = ExecutionDays
                .OrderBy(day => day)
                .Select(day => (LastExecution ?? now.MinusDays(1)).LocalDateTime.Next(day))
                .Select(date => date.Date.At(ExecutionTime))
                .FirstOrDefault();
            
            return nextExecution == default ? null : nextExecution.InZoneStrictly(now.Zone);
        }
    }
}
