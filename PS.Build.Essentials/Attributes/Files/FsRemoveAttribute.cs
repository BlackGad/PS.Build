using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FsRemoveAttribute : Attribute
    {
        #region Constructors

        public FsRemoveAttribute(string sourcePattern, string filterPattern = null)
        {
        }

        #endregion
    }
}