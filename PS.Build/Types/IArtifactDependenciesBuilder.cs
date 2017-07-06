namespace PS.Build.Types
{
    public interface IArtifactDependenciesBuilder
    {
        #region Members

        IArtifactDependenciesBuilder FileDependency(string path);
        IArtifactDependenciesBuilder TagDependency(string tag);

        #endregion
    }
}