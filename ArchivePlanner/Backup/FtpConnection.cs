using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace ArchivePlanner.Backup
{
    //https://datatracker.ietf.org/doc/html/rfc959
    public class FtpConnection
    {
        private readonly Uri _uri;
        private readonly X509Certificate _certificate;
        private readonly NetworkCredential _credentials;

        public FtpConnection(Uri url, X509Certificate certificate, NetworkCredential credentials)
        {
            _uri = url;
            _certificate = certificate;
            _credentials = credentials;

            UseSSL = true;
        }

        public bool UseSSL { get; set; }

        public void UploadFileToFolder(string folder, FileInfo file)
        {
            var destinationUri = new Uri(_uri, Path.Combine(folder, file.Name));
            var request = (FtpWebRequest)WebRequest.Create(destinationUri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.EnableSsl = UseSSL;
            request.Credentials = _credentials;

            ServicePointManager.ServerCertificateValidationCallback =
                (s, certificate, chain, sslPolicyErrors) =>
                {
                    return _certificate.Equals(certificate);
                };

            using (var requestStream = request.GetRequestStream())
            using (var sourceStream = file.OpenRead())
            {
                sourceStream.CopyTo(requestStream);
            }

            using var response = (FtpWebResponse)request.GetResponse();
            
            if(response.StatusCode == FtpStatusCode.ClosingData)
            {
                throw new Exception("File could not be uploaded");
            }
        }

        public Stream OpenUploadStream(string relativePath)
        {
            var destinationUri = new Uri(_uri, relativePath);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(destinationUri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.EnableSsl = UseSSL;
            request.Credentials = _credentials;

            ServicePointManager.ServerCertificateValidationCallback =
                (s, certificate, chain, sslPolicyErrors) =>
                {
                    return _certificate.Equals(certificate);
                };

            return request.GetRequestStream();
        }
    }
}
