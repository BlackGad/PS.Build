using PS.Build.Types;

namespace PS.Build.Services
{
    public interface IArtifactory
    {
        #region Members

        IArtifactBuilder Artifact(string path, BuildItem type);

        #endregion
    }
}