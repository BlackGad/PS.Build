using PS.Build.Types;

namespace PS.Build.Services
{
    public interface INugetExplorer
    {
        #region Members

        INugetPackage FindPackage(string packageId);

        #endregion
    }
}