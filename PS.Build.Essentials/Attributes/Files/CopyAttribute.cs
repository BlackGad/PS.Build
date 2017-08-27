using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class CopyAttribute : Attribute
    {
        #region Constructors

        public CopyAttribute(string sourcePattern, string target, string filterPattern = null)
        {
        }

        #endregion
    }
}