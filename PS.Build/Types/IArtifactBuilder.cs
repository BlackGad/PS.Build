using System;

namespace PS.Build.Types
{
    public interface IArtifactBuilder
    {
        #region Members

        IArtifactBuilder Content(Func<byte[]> contentFactory);
        IArtifactDependenciesBuilder Dependencies();

        IArtifactBuilder Metadata(string type, string value);
        IArtifactBuilder Permanent();

        #endregion
    }
}