using ArchivePlanner.Util;
using System;
using System.Security;
using System.Text.Json.Serialization;

namespace ArchivePlanner.Planning.Model
{
    public class FtpConnection : ICloneable
    {
        private string? _password;

        public FtpConnection()
        {
        }

        [JsonConstructor]
        public FtpConnection(string host, string username, string? ecryptedPassword)
        {
            Host = host;
            Username = username;
            EcryptedPassword = ecryptedPassword;
        }

        public string Host { get; set; } = null!;

        public string Username { get; set; } = null!;

        [JsonInclude]
        public string? EcryptedPassword
        {
            get => _password;
            init => _password = value;
        }

        [JsonIgnore]
        public SecureString? Password
        {
            get
            {
                if(_password is null)
                {
                    return null;
                }

                var entropy = System.Text.Encoding.Unicode.GetBytes("jo3ldkKI22!");
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                                        Convert.FromBase64String(_password),
                                        entropy,
                                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return Security.ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            set
            {
                var entropy = System.Text.Encoding.Unicode.GetBytes("jo3ldkKI22!");
                byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                                        System.Text.Encoding.Unicode.GetBytes(Security.ToInsecureString(value!)),
                                        entropy,
                                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                _password = Convert.ToBase64String(encryptedData);
            }
        }

        public object Clone()
        {
            return new FtpConnection(Host, Username, EcryptedPassword);
        }
    }
}
