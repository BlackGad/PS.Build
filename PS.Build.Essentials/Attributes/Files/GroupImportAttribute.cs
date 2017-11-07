using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class GroupImportAttribute : GroupBaseAttribute
    {
        private readonly string _pattern;

        #region Constructors

        public GroupImportAttribute(string pattern)
        {
            _pattern = pattern ?? string.Empty;
        }

        #endregion
    }
}