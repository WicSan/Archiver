using Archiver.Backup;
using Archiver.Planning.Model;
using FluentFTP;
using FluentFTP.Client.BaseClient;
using FluentFTP.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Archiver.Planning
{
    public class FtpClientFactory : IFtpClientFactory
    {
        private readonly X509Certificate _cert;
        private readonly bool _enabledCommandLogging;
        private readonly ILogger<FtpClient> _logger;

        public FtpClientFactory(bool enabledCommandLogging, ILogger<FtpClient> logger)
        {
            _cert = X509Certificate.CreateFromCertFile("Resources/ftp.crt");
            _enabledCommandLogging = enabledCommandLogging;
            _logger = logger;
        }

        public IAsyncFtpClient CreateFtpClient(FtpConnectionDetails connection)
        {
            if (connection.Host is null || connection.Username is null || connection.Password is null)
            {
                throw new InvalidOperationException();
            }

            var credentials = new NetworkCredential(connection.Username, connection.Password);
            var config = new FtpConfig
            {
                EncryptionMode = FtpEncryptionMode.Explicit,
                DataConnectionType = FtpDataConnectionType.AutoPassive,
                StaleDataCheck = false,
                ValidateAnyCertificate = true,
                RetryAttempts = 3,
                LogToConsole = _enabledCommandLogging,
                LogPassword = false
            };

            var logAdapter = new FtpLogAdapter(_logger);
            var client = new AsyncFtpClient(connection.Host, credentials, 21, config, logAdapter);
            client.ValidateCertificate += (BaseFtpClient control, FtpSslValidationEventArgs e) =>
            {
                if (e.Certificate.Equals(_cert))
                {
                    e.Accept = true;
                }
                else
                {
                    e.Accept = false;
                }
            };

            return client;
        }
    }
}
