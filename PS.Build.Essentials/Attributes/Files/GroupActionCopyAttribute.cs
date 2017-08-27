using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class GroupActionCopyAttribute : GroupBaseActionAttribute
    {
        #region Constructors

        public GroupActionCopyAttribute(string targetFolder)
        {
        }

        #endregion
    }
}