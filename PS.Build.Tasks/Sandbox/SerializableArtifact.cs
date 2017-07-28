using System;
using System.Collections.Generic;
using PS.Build.Types;

namespace PS.Build.Tasks
{
    [Serializable]
    class SerializableArtifact
    {
        #region Properties

        public bool IsPermanent { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
        public string Path { get; set; }
        public BuildItem Type { get; set; }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"({Type}) {Path}";
        }

        #endregion
    }
}