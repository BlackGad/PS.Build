using System;

namespace DefinitionAttributeInitialization
{
    public abstract class BaseCustomAttribute : Attribute
    {
        #region Properties

        public string ID { get; set; }

        #endregion
    }
}