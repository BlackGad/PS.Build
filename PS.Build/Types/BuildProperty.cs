﻿using System;

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
        RootNamespace,

        /// <summary>
        ///     The absolute path name of the primary output file for the build (defined as drive + path + base name + file
        ///     extension).
        /// </summary>
        TargetPath,

        /// <summary>
        ///     The base name of the primary output file for the build.
        /// </summary>
        TargetName,

        /// <summary>
        ///     The file extension of the primary output file for the build. It includes the '.' before the file extension.
        /// </summary>
        TargetExtension,

        /// <summary>
        ///     Target framework is the particular version of the .NET Framework that your project is built to run on.
        /// </summary>
        TargetFrameworkVersion,

        /// <summary>
        ///     Defines conditional compiler constants. Symbol/value pairs are separated by semicolons and are specified by using
        ///     the following syntax: symbol1 = value1; symbol2 = value2
        ///     The property is equivalent to the /define compiler switch.
        /// </summary>
        DefineConstants
    }
}