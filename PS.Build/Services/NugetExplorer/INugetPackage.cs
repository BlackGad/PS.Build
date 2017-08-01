using System;

namespace PS.Build.Types
{
    /// <summary>
    ///     Represents generic Nuget package information.
    /// </summary>
    public interface INugetPackage
    {
        #region Properties

        /// <summary>
        ///     Gets stored folder.
        /// </summary>
        string Folder { get; }

        /// <summary>
        ///     Gets Nuget package ID.
        /// </summary>
        string ID { get; }

        /// <summary>
        ///     Gets Nuget package version.
        /// </summary>
        Version Version { get; }

        #endregion
    }
}