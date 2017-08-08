using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FgActionCopyAttribute : FgBaseActionAttribute
    {
        #region Constructors

        public FgActionCopyAttribute(string targetFolder)
        {
        }

        #endregion
    }
}