using PS.Build.Types;

namespace PS.Build.Services
{
    /// <summary>
    ///     Nuget explorer service povider.
    /// </summary>
    public interface INugetExplorer
    {
        #region Members

        /// <summary>
        ///     Returns Nuget package information for package that was already installed (Any project in solution).
        /// </summary>
        /// <param name="packageId">Nuget package ID</param>
        /// <returns>Installed package information or null if package was not found.</returns>
        INugetPackage FindPackage(string packageId);

        #endregion
    }
}