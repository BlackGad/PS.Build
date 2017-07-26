namespace PS.Build.Types
{
    /// <summary>
    ///     Fluent MSBuild item content dependency builder.
    /// </summary>
    public interface IArtifactDependenciesBuilder
    {
        #region Members

        /// <summary>
        ///     Add file as dependency. File modified time will be analyzed.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>Fluent provider.</returns>
        IArtifactDependenciesBuilder FileDependency(string path);

        /// <summary>
        ///     Add custom tag as dependency. Hash code from tag will be analyzed.
        /// </summary>
        /// <param name="tag">Custom string variable.</param>
        /// <returns>Fluent provider.</returns>
        IArtifactDependenciesBuilder TagDependency(string tag);

        #endregion
    }
}