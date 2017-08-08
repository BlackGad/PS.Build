using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FsCopyAttribute : Attribute
    {
        #region Constructors

        public FsCopyAttribute(string sourcePattern, string target, string filterPattern = null)
        {
        }

        #endregion
    }
}