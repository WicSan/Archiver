using ArchivePlanner.Planning.Database;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace ArchivePlanner.Planning.Model
{
    public class BackupPlan : IEquatable<BackupPlan>, ICloneable
    {
        [IdAttribute]
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public FtpConnection Connection { get; set; }

        public string DestinationFolder { get; set; } = null!;

        public BackupSchedule Schedule { get; set; }

        public ICollection<FileSystemInfo> FileSystemItems { get; set; } = new List<FileSystemInfo>();

        [JsonIgnore]
        public double Progress { get; set; } = -1;

        public BackupPlan()
        {
            Connection = new FtpConnection();
            Schedule = new DailyBackupSchedule(LocalTime.Noon);
        }

        public BackupPlan(BackupPlan plan) : this()
        {
            Name = plan.Name;
            DestinationFolder = plan.DestinationFolder;
            FileSystemItems = plan.FileSystemItems;
            Schedule = (BackupSchedule)plan.Schedule.Clone();
            Connection = (FtpConnection)plan.Connection.Clone();
        }

        public string FullName
        {
            get
            {
                var timestamp = SystemClock.Instance.GetCurrentInstant().ToString("yyyy-MM-dd-HH-mm-ss", null);
                var backupFileName = $"{Name}_{timestamp}";
                var backupFullName = $"{DestinationFolder}/{backupFileName}";

                if (backupFullName.StartsWith("/"))
                {
                    backupFullName = backupFullName.Remove(0, 1);
                }
                
                return backupFullName;
            }
        }

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

        public object Clone()
        {
            return new BackupPlan(this);
        }
    }
}
