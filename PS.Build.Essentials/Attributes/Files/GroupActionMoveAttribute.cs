using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class GroupActionMoveAttribute : GroupBaseActionAttribute
    {
        #region Constructors

        public GroupActionMoveAttribute(string targetFolder)
        {
        }

        #endregion
    }
}