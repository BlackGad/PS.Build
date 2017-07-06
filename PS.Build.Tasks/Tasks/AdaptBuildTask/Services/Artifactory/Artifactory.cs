using System;
using System.Collections.Generic;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class Artifactory : IArtifactory
    {
        #region Constructors

        public Artifactory()
        {
            Artifacts = new List<Artifact>();
        }

        #endregion

        #region Properties

        public List<Artifact> Artifacts { get; }

        #endregion

        #region IArtifactory Members

        IArtifactBuilder IArtifactory.Artifact(string path, BuildItem type)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            path = path.ToLowerInvariant();

            var artifact = new Artifact(path, type);
            if (Artifacts.Contains(artifact)) throw new Exception($"Artifact {artifact} multiple entry");
            Artifacts.Add(artifact);
            return artifact;
        }

        #endregion
    }
}