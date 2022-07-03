using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Archiver.Tests
{
    public class StaFactDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly FactDiscoverer factDiscoverer;

        public StaFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            factDiscoverer = new FactDiscoverer(diagnosticMessageSink);
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return factDiscoverer.Discover(discoveryOptions, testMethod, factAttribute)
                                 .Select(testCase => new StaTestCase(testCase));
        }
    }
}
