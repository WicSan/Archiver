using Archiver.Backup;
using Archiver.Planning.Database;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Archiver.Planning.Model
{
    public class BackupPlan : IEquatable<BackupPlan>, ICloneable
    {
        [IdAttribute]
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public FtpConnectionDetails Connection { get; set; }

        public string DestinationFolder { get; set; } = null!;

        public BackupSchedule Schedule { get; set; }

        public ICollection<FileSystemInfo> FileSystemItems { get; set; } = new List<FileSystemInfo>();

        [JsonIgnore]
        public double Progress { get; set; } = -1;

        public BackupPlan()
        {
            Connection = new FtpConnectionDetails();
            Schedule = new DailyBackupSchedule(LocalTime.Noon);
        }

        public BackupPlan(BackupPlan plan) : this()
        {
            Name = plan.Name;
            DestinationFolder = plan.DestinationFolder;
            FileSystemItems = plan.FileSystemItems;
            BackupType = plan.BackupType;
            Schedule = (BackupSchedule)plan.Schedule.Clone();
            Connection = (FtpConnectionDetails)plan.Connection.Clone();
        }

        [JsonIgnore]
        public string FullName
        {
            get
            {
                var timestamp = SystemClock.Instance.GetCurrentInstant().ToString("yyyy-MM-dd-HH-mm-ss", null);
                var backupFileName = $"{Name}_{timestamp}";
                return $"{DestinationFolder}/{backupFileName}";
            }
        }

        public bool InitialBackupExecuted => Schedule.LastExecution is not null;

        public BackupType BackupType { get; set; }

        public override bool Equals(object? obj) => this.Equals(obj as BackupPlan);


        public bool Equals(BackupPlan? other)
        {
            return Name.Equals(other?.Name, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public void ResetProgress()
        {
            Progress = -1;
        }

        public void UpdateExecutionTime(LocalDateTime dateTime)
        {
            Schedule.LastExecution = dateTime;
        }

        public object Clone()
        {
            return new BackupPlan(this);
        }
    }
}
