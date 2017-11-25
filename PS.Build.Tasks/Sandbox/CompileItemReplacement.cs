using System;

namespace PS.Build.Tasks
{
    [Serializable]
    class CompileItemReplacement
    {
        #region Properties

        public string Source { get; set; }
        public string Target { get; set; }

        #endregion
    }
}