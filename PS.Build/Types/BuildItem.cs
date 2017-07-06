using System;

namespace PS.Build.Types
{
    [Serializable]
    public enum BuildItem
    {
        /// <summary>
        ///     Internal item.
        /// </summary>
        Internal,
        None,
        Compile,
        EmbeddedResource,
        Resource,
        Content,
        AdditionalFiles,
        Page,
        EntityDeploy
    }
}