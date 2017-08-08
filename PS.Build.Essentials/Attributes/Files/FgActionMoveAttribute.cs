using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FgActionMoveAttribute : FgBaseActionAttribute
    {
        #region Constructors

        public FgActionMoveAttribute(string targetFolder)
        {
        }

        #endregion
    }
}