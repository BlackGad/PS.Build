using System;

namespace PS.Build.Essentials.Attributes
{
    public abstract class GroupBaseAttribute : Attribute
    {
        #region Properties

        public string Group { get; set; }

        #endregion
    }
}