using System;
using System.ComponentModel;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class GroupActionPackAttribute : GroupBaseActionAttribute
    {
        #region Constructors

        public GroupActionPackAttribute(string archivePath)
        {
        }

        #endregion
    }
}