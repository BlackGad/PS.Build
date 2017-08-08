using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FgSelectFromArchiveAttribute : FgBaseAttribute
    {
        #region Constructors

        public FgSelectFromArchiveAttribute(string archivePath, string pattern)
        {
        }

        #endregion
    }
}