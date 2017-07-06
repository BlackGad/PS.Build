using System;

namespace PS.Build.Types
{
    public interface INugetPackage
    {
        #region Properties

        string Folder { get; }
        string ID { get; }
        Version Version { get; }

        #endregion
    }
}