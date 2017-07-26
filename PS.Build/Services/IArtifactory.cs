using PS.Build.Types;

namespace PS.Build.Services
{
    /// <summary>
    ///     Artifactory service interface. Used to interact with MSBuild project on PreBuild action.
    /// </summary>
    public interface IArtifactory
    {
        #region Members

        /// <summary>
        ///     Declare additional artifact MSBuild Item that will be included to build process before compilation.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="type">MSBuild item type</param>
        /// <returns>Fluent artifact item builder</returns>
        IArtifactBuilder Artifact(string path, BuildItem type);

        #endregion
    }
}