using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit.Sdk;

namespace Xunit.Runner.VisualStudio
{
    class VsSourceTestDiscoverer : ISourceTestDiscoverer
    {
        public Task AnalyzeDocumentAsync(ISourceDiscoveryContext context, SemanticModel semanticModel, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            
            if (!TryGetXunitExecutionAssemblyPath(AppDomainSupport.Denied, semanticModel.Compilation, out var xunitExecutionAssemblyPath))
                return Task.CompletedTask;

            using (var discoverySink = new TestDiscoverySink(() => cancellation.IsCancellationRequested))
            using (var xunit2 = new Xunit2Discoverer(
                AppDomainSupport.Denied,
                new NullSourceInformationProvider(),
                new SourceAssemblyInfo(semanticModel, cancellation),
                xunitExecutionAssemblyPath,
                verifyAssembliesOnDisk: false))
            {
                var configuration = context.ConfigurationFile != null ? ConfigurationHelper.LoadConfiguration(ConfigurationHelper.GetAssemblyFileName(context.Source), context.ConfigurationFile) : null;
                xunit2.Find(includeSourceInformation: false, messageSink: discoverySink, discoveryOptions: TestFrameworkOptions.ForDiscovery(configuration));

                while (!discoverySink.Finished.WaitOne(50))
                    cancellation.ThrowIfCancellationRequested();

                TrySendDiscoveredTestCases(context, discoverySink, ExecutorUri, cancellation);
            }

            return Task.CompletedTask;
        }

        public async Task AnalyzeProjectAsync(ISourceDiscoveryContext context, Compilation compilation, CancellationToken cancellation)
        {
            foreach (var semanticModel in compilation.SyntaxTrees.Select(tree => compilation.GetSemanticModel(tree)))
                await AnalyzeDocumentAsync(context, semanticModel, cancellation).ConfigureAwait(false);
        }

        public Uri ExecutorUri => new Uri("executor://xunit/VsTestRunner2");

        static bool TryGetXunitExecutionAssemblyPath(AppDomainSupport appDomainSupport, Compilation compilation, out string xunitExecutionAssemblyPath)
        {
            // TODO: Use GetSupportedPlatformSuffixes
            //var supportedPlatformSuffixes = Xunit2Discoverer.GetSupportedPlatformSuffixes(appDomainSupport);
            var supportedPlatformSuffixes = new[] { "dotnet", "desktop" };

            foreach (var suffix in supportedPlatformSuffixes)
            {
                var fileName = $"xunit.execution.{suffix}.dll";
                foreach (var reference in compilation.References)
                    if (reference.Display != null && reference.Display.EndsWith(fileName))
                    {
                        xunitExecutionAssemblyPath = reference.Display;
                        return true;
                    }
            }

            xunitExecutionAssemblyPath = null;
            return false;
        }

        static void TrySendDiscoveredTestCases(ISourceDiscoveryContext context, TestDiscoverySink discoverySink, Uri executorUri, CancellationToken cancellation)
        {
            foreach (var test in discoverySink.TestCases)
            {
                cancellation.ThrowIfCancellationRequested();

                var testCase = new TestCase(
                    $"{test.TestMethod.TestClass.Class.Name}.{test.TestMethod.Method.Name}",
                    executorUri,
                    context.Source)
                {
                    DisplayName = test.DisplayName,
                    CodeFilePath = test.SourceInformation?.FileName,
                    LineNumber = test.SourceInformation?.LineNumber ?? -1
                };

                var traits = test.Traits;
                foreach (var key in traits.Keys)
                    foreach (var value in traits[key])
                        testCase.Traits.Add(new Trait(key, value));

                context.ReportDiscoveredTest(testCase);
            }
        }
    }
}
