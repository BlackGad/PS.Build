using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FgForkAttribute : FgBaseAttribute
    {
        private readonly string _pattern;
        private readonly string _sourceGroup;

        #region Constructors

        public FgForkAttribute(string pattern, string sourceGroup)
        {
            _sourceGroup = sourceGroup;
            _pattern = pattern ?? string.Empty;
        }

        #endregion
    }
}