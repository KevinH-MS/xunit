using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

// TODO: this should be moved to the objectmodel dll...

namespace Microsoft.VisualStudio.TestPlatform.ObjectModel
{
    /// <summary>
    /// Context for source-based test discovery analysis.
    /// </summary>
    public interface ISourceDiscoveryContext
    {
        /// <summary>
        /// Path to configuration file for this <see cref="Source"/>.
        /// </summary>
        string ConfigurationFile { get; }

        /// <summary>
        /// Value to use when initializing <see cref="TestCase.Source"/> property (expected to be the project's full "bin" output path).
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Reports the discovery of a <paramref name="test"/> defined in the supplied <paramref name="tree"/>.
        /// </summary>
        void ReportDiscoveredTest(TestCase test);
    }

    public interface ISourceTestDiscoverer
    {
        /// <summary>
        /// The Uri of the test adapter that handles tests for this discoverer.
        /// </summary>
        Uri ExecutorUri { get; }

        /// <summary>
        /// Performs discovery on the supplied <paramref name="semanticModel"/> and calls
        /// <see cref="ISourceDiscoveryContext.ReportDiscoveredTest(TestCase)"/> to report discovered tests.
        /// </summary>
        /// <remarks>
        /// Must cancel discovery if cancellation is triggered via <paramref name="cancellation"/>.
        /// </remarks>
        Task AnalyzeDocumentAsync(ISourceDiscoveryContext context, SemanticModel semanticModel, CancellationToken cancellation);

        /// <summary>
        /// Performs discovery on the supplied <paramref name="compilation"/> and calls
        /// <see cref="ISourceDiscoveryContext.ReportDiscoveredTest(TestCase)"/> to report discovered tests.
        /// </summary>
        /// <remarks>
        /// Must cancel discovery if cancellation is triggered via <paramref name="cancellation"/>.
        /// </remarks>
        Task AnalyzeProjectAsync(ISourceDiscoveryContext context, Compilation comilation, CancellationToken cancellation);
    }
}