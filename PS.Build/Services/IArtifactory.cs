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
        ///     Declare artifact MSBuild Item.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="type">MSBuil item type</param>
        /// <returns>Fluent artifact item builder</returns>
        IArtifactBuilder Artifact(string path, BuildItem type);

        #endregion
    }
}