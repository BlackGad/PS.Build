using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class RemoveAttribute : Attribute
    {
        #region Constructors

        public RemoveAttribute(string sourcePattern, string filterPattern = null)
        {
        }

        #endregion
    }
}