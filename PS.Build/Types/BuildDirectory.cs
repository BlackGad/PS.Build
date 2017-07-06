using System;

namespace PS.Build.Types
{
    [Serializable]
    public enum BuildDirectory
    {
        Solution,
        Project,
        Target,
        Intermediate
    }
}