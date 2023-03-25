using Archiver.Planning.Model;
using FluentFTP;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Archiver.Planning
{
    public class FtpClientFactory : IFtpClientFactory
    {
        private readonly X509Certificate _cert;
        private readonly bool _enabledLogging;
        private readonly ILogger<FtpClient> _logger;

        public FtpClientFactory(bool enabledLogging, ILogger<FtpClient> logger)
        {
            _cert = X509Certificate.CreateFromCertFile("Resources/ftp.crt");
            _enabledLogging = enabledLogging;
            _logger = logger;
        }

        public IFtpClient CreateFtpClient(FtpConnectionDetails connection)
        {
            if (connection.Host is null || connection.Username is null || connection.Password is null)
            {
                throw new InvalidOperationException();
            }

            var credentials = new NetworkCredential(connection.Username, connection.Password);
            var client = new FtpClient(connection.Host, credentials);
            client.EncryptionMode = FtpEncryptionMode.Explicit;
            client.Port = 21;
            client.DataConnectionType = FtpDataConnectionType.PASV;
            client.ValidateAnyCertificate = true;
            client.RetryAttempts = 3;
            client.ValidateCertificate += (FtpClient control, FtpSslValidationEventArgs e) =>
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

            if (_enabledLogging)
            {
                client.OnLogEvent += (tracelevel, message) =>
                {
                    switch (tracelevel)
                    {
                        case FtpTraceLevel.Error:
                            _logger.LogError(message);
                            break;
                        case FtpTraceLevel.Verbose:
                            _logger.LogDebug(message);
                            break;
                        case FtpTraceLevel.Warn:
                            _logger.LogWarning(message);
                            break;
                        case FtpTraceLevel.Info:
                        default:
                            _logger.LogInformation(message);
                            break;
                    }
                };
            }

            return client;
        }
    }
}
