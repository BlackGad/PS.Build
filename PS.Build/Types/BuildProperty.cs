using System;

namespace PS.Build.Types
{
    /// <summary>
    ///     Specifies enumerated constants used to retrieve common MSBuild properties.
    /// </summary>
    /// <see cref="https://msdn.microsoft.com/en-us/library/bb629394.aspx" />
    [Serializable]
    public enum BuildProperty
    {
        /// <summary>
        ///     Represents project filename.
        /// </summary>
        ProjectFile,

        /// <summary>
        ///     The configuration that you are building, either "Debug" or "Release."
        /// </summary>
        Configuration,

        /// <summary>
        ///     The operating system you are building for. Valid values are "Any CPU", "x86", and "x64".
        /// </summary>
        Platform,

        /// <summary>
        ///     The root namespace to use when you name an embedded resource. This namespace is part of the embedded resource
        ///     manifest name.
        /// </summary>
        RootNamespace
    }
}