using System;

namespace PS.Build.Types
{
    /// <summary>
    ///     Specifies enumerated constants used to retrieve directory paths to MSBuild special folders.
    /// </summary>
    [Serializable]
    public enum BuildDirectory
    {
        /// <summary>
        ///     The directory of the solution (defined as drive + path); includes the trailing backslash '\'.
        /// </summary>
        Solution,

        /// <summary>
        ///     The directory of the project (defined as drive + path); includes the trailing backslash '\'.
        /// </summary>
        Project,

        /// <summary>
        ///     The directory of the primary output file for the build (defined as drive + path); includes the trailing backslash
        ///     '\'.
        /// </summary>
        Target,

        /// <summary>
        ///     Path to the directory specified for intermediate files.
        /// </summary>
        Intermediate
    }
}