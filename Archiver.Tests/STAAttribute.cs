using System;
using Xunit;
using Xunit.Sdk;

namespace Archiver.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [XunitTestCaseDiscoverer("Archiver.Tests.StaFactDiscoverer", "Archiver.Tests")]
    public class STAFactAttribute : FactAttribute
    {
    }
}
