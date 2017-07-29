using System;

namespace PS.Build.Types
{
    /// <summary>
    ///     Build step flags
    /// </summary>
    [Flags]
    public enum BuildStep
    {
        /// <summary>
        ///     Build step not specified
        /// </summary>
        None,

        /// <summary>
        ///     Pre build step
        /// </summary>
        PreBuild = 0x1,

        /// <summary>
        ///     Post build step
        /// </summary>
        PostBuild = PreBuild << 0x1
    }
}