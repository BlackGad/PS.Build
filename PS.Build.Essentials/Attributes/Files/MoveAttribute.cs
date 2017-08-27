using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class MoveAttribute : Attribute
    {
        #region Constructors

        public MoveAttribute(string sourcePattern, string targetFolder, string filterPattern = null)
        {
        }

        #endregion
    }
}