using System;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    public class NugetPackage : INugetPackage
    {
        #region Constructors

        public NugetPackage(string id, Version version, string folder)
        {
            Folder = folder;
            ID = id;
            Version = version;
        }

        #endregion

        #region Properties

        public string Folder { get; }
        public string ID { get; }
        public Version Version { get; }

        #endregion

        public override string ToString()
        {
            return $"[{Version}] {ID}";
        }
    }
}