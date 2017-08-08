using System;

namespace PS.Build.Essentials.Attributes
{
    public abstract class FgBaseAttribute : Attribute
    {
        #region Properties

        public string Group { get; set; }

        #endregion
    }
}