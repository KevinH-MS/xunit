using System.IO;
using Xunit;

static class ConfigurationHelper
{
    public static string GetAssemblyFileName(string source)
    {
#if !WINDOWS_UAP
        return Path.GetFullPath(source);
#else
            return source;
#endif
    }

    public static TestAssemblyConfiguration LoadConfiguration(string assemblyName, string configFileName = null)
    {
#if WINDOWS_UAP
            var stream = GetConfigurationStreamForAssembly(assemblyName);
            return stream == null ? new TestAssemblyConfiguration() : ConfigReader.Load(stream);
#else
        return ConfigReader.Load(assemblyName, configFileName);
#endif
    }
}
