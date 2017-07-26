using System;

namespace PS.Build.Types
{
    /// <summary>
    ///     Specifies enumerated constants used to retrieve common MSBuild project items.
    /// </summary>
    /// <see cref="https://msdn.microsoft.com/en-us/library/bb629388.aspx" />
    /// <see cref="https://stackoverflow.com/questions/145752/what-are-the-various-build-action-settings-in-visual-studio-project-properties" />
    [Serializable]
    public enum BuildItem
    {
        /// <summary>
        ///     Represents internal files that should have no role in the build process and will not be included to any MSBuild
        ///     item list.
        /// </summary>
        Internal,

        /// <summary>
        ///     Represents files that should have no role in the build process.
        /// </summary>
        None,

        /// <summary>
        ///     Represents the source files for the compiler.
        /// </summary>
        Compile,

        /// <summary>
        ///     Represents resources to be embedded in the generated assembly.
        /// </summary>
        EmbeddedResource,

        /// <summary>
        ///     Embeds the file in a shared (by all files in the assembly with similar setting) assembly manifest resource named
        ///     AppName.g.resources.
        /// </summary>
        Resource,

        /// <summary>
        ///     Represents files that are not compiled into the project, but may be embedded or published together with it.
        /// </summary>
        Content,

        /// <summary>
        ///     Represents additional files.
        /// </summary>
        AdditionalFiles,

        /// <summary>
        ///     Used to compile a xaml file into baml. The baml is then embedded with the same technique as Resource (i.e.
        ///     available as `AppName.g.resources)
        /// </summary>
        Page,

        /// <summary>
        ///     Represents files that will be deploed with EntityDeploy task
        /// </summary>
        EntityDeploy
    }
}