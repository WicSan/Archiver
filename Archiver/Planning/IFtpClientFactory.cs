using Archiver.Planning.Model;
using FluentFTP;

namespace Archiver.Planning
{
    public interface IFtpClientFactory
    {
        FtpClient CreateFtpClient(FtpConnection connection);
    }
}