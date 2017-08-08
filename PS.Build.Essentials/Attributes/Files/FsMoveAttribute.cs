using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FsMoveAttribute : Attribute
    {
        #region Constructors

        public FsMoveAttribute(string sourcePattern, string targetFolder, string filterPattern = null)
        {
        }

        #endregion
    }
}