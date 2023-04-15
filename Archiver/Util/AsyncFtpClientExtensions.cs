using FluentFTP;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Util
{
    public static class AsyncFtpClientExtensions
    {
        public static async Task Connect(this IAsyncFtpClient client, int retryAttempts = 1, CancellationToken token = default)
        {
            await Observable
                .FromAsync(() => client.Connect(token))
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
