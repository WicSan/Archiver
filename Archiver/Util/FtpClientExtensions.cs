using FluentFTP;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArchivePlanner.Util
{
    public static class FtpClientExtensions
    {
        public static async Task ConnectAsync(this FtpClient client, int retryAttempts = 1, CancellationToken token = default)
        {
            await Observable
                .FromAsync(() => client.ConnectAsync(token))
                .RetryWhen(o => Observable
                   .Range(0, retryAttempts)
                   .Zip(o, (attempt, e) => (Attempt: attempt, Exception: e))
                   .SelectMany(tuple =>
                   {
                       if (tuple.Attempt == retryAttempts - 1)
                       {
                           throw tuple.Exception;
                       }
                       return Observable.Timer(TimeSpan.FromSeconds(tuple.Attempt));
                   }));
        }
    }
}
