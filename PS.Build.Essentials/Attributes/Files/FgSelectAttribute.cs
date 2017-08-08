using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FgSelectAttribute : FgBaseAttribute
    {
        private readonly string _pattern;

        #region Constructors

        public FgSelectAttribute(string pattern)
        {
            _pattern = pattern ?? string.Empty;
        }

        #endregion
    }
}