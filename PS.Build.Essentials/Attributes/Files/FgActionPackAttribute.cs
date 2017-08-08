using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FgActionPackAttribute : FgBaseActionAttribute
    {
        #region Constructors

        public FgActionPackAttribute(string archivePath, string relativePath = null)
        {
        }

        #endregion
    }
}