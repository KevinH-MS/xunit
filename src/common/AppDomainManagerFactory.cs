namespace Xunit
{
    internal static class AppDomainManagerFactory
    {
        internal static IAppDomainManager Create(bool useAppDomain, string assemblyFileName, string configFileName, bool shadowCopy, string shadowCopyFolder)
        {
#if !NO_APPDOMAIN
            if (useAppDomain)
                return new AppDomainManager_AppDomain(assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
#endif

            return new AppDomainManager_NoAppDomain(assemblyFileName);
        }
    }
}
