using Microsoft.Extensions.Logging;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit.Abstractions;

namespace Archiver.Tests
{
    public class LoggerMock<T> : ILogger<T>
    {
        private readonly ReplaySubject<string> subject = new();
        private readonly ITestOutputHelper _output;

        public LoggerMock()
        {
        }

        public LoggerMock(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void WaitUntil(string text, int timeout = int.MaxValue)
        {
            Observable.Create<Unit>(o =>
                {
                    o.OnNext(new Unit());
                    return subject
                        .Subscribe(t =>
                        {
                            if (t.Equals(text, StringComparison.OrdinalIgnoreCase))
                            {
                                o.OnCompleted();
                            }
                        });
                })
                .Timeout(TimeSpan.FromMilliseconds(timeout))
                .Wait();
        }

        public void Log<TState>(LogLevel level, EventId id, TState state, Exception e, Func<TState, Exception, string> f)
        {
            subject.OnNext(state.ToString());

            _output?.WriteLine("{0}: {1}", level, state);
        }
    }
}
