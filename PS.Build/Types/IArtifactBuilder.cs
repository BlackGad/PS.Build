using System;

namespace PS.Build.Types
{
    /// <summary>
    ///     Fluent MSBuild item artifact builder.
    /// </summary>
    public interface IArtifactBuilder
    {
        #region Members

        /// <summary>
        ///     Define dynamic item content factory.
        /// </summary>
        /// <param name="contentFactory">Content factory.</param>
        /// <returns>Fluent provider.</returns>
        IArtifactBuilder Content(Func<byte[]> contentFactory);

        /// <summary>
        ///     Specify item content dependencies. If dependencies were changed beetwen compilations content will be regenerated.
        /// </summary>
        /// <returns>Artifact dependency fluent provider.</returns>
        IArtifactDependenciesBuilder Dependencies();

        /// <summary>
        ///     Add MSBuild item metadata.
        /// </summary>
        /// <param name="type">Metadata type.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>Fluent provider.</returns>
        IArtifactBuilder Metadata(string type, string value);

        /// <summary>
        ///     Mark MSBuild item as permanent. This is basically means that artifact will not be cleaned up while project clean
        ///     phase.
        /// </summary>
        /// <returns>Fluent provider.</returns>
        IArtifactBuilder Permanent();

        #endregion
    }
}