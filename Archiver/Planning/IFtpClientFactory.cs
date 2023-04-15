using Archiver.Planning.Model;
using FluentFTP;

namespace Archiver.Planning
{
    public interface IFtpClientFactory
    {
        IAsyncFtpClient CreateFtpClient(FtpConnectionDetails connection);
    }
}