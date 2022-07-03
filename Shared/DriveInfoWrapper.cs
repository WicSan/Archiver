using System;
using System.IO;

namespace Archiver.Shared
{
    public class DriveInfoWrapper : FileSystemInfo
    {
        private readonly DriveInfo _wrapped;

        public DriveInfoWrapper(string name)
        {
            _wrapped = new DriveInfo(name);
        }

        public DriveInfoWrapper(DriveInfo wrapped)
        {
            _wrapped = wrapped;
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override bool Exists { get; }

        public override string Name => _wrapped.Name;

        public override string FullName => _wrapped.Name;
    }
}
