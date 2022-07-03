using ArchivePlanner.Planning.Model;
using FluentFTP;

namespace ArchivePlanner.Planning
{
    public interface IFtpClientFactory
    {
        FtpClient CreateFtpClient(FtpConnection connection);
    }
}