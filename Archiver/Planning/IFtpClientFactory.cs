using Archiver.Planning.Model;
using FluentFTP;

namespace Archiver.Planning
{
    public interface IFtpClientFactory
    {
        IFtpClient CreateFtpClient(FtpConnectionDetails connection);
    }
}