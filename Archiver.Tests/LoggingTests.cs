using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace Archiver.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void TestLoggingMock()
        {
            var loggerMock = new LoggerMock<LoggingTests>();

            loggerMock.LogInformation("test");

            loggerMock.WaitUntil("test");
        }
    }
}
