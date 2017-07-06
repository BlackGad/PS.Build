using System;

namespace PS.Build.Tasks.Services
{
    [Serializable]
    public class ArtifactCache
    {
        #region Properties

        public int[] HashCodes { get; set; }

        #endregion
    }
}