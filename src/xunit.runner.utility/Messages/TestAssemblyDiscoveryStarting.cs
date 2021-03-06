using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyDiscoveryStarting"/>.
    /// </summary>
    public class TestAssemblyDiscoveryStarting : ITestAssemblyDiscoveryStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyDiscoveryStarting"/> class.
        /// </summary>
        /// <param name="assembly">Information about the assembly that is being discovered</param>
        /// <param name="appDomain">Indicates whether the tests will be discovered and run in a separate app domain</param>
        /// <param name="discoveryOptions">The discovery options</param>
        public TestAssemblyDiscoveryStarting(XunitProjectAssembly assembly,
                                             bool appDomain,
                                             ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            Assembly = assembly;
            AppDomain = appDomain;
            DiscoveryOptions = discoveryOptions;
        }

        /// <inheritdoc/>
        public bool AppDomain { get; private set; }

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkDiscoveryOptions DiscoveryOptions { get; private set; }
    }
}
