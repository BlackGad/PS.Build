using System;
using System.ComponentModel;

namespace DefinitionAttributeInitialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class CustomAttribute : Attribute
    {
        #region Constructors

        public CustomAttribute(uint aaaaaa, int a = 0, string b = null, float c = 0, string def = null, params byte[] ssss)
        {
        }

        public CustomAttribute()
        {
        }

        public CustomAttribute(uint aaaaaa, int a = 0)
        {
        }

        #endregion

        #region Properties

        public double D { get; set; }
        public Double D2 { get; set; }

        #endregion

        #region Members

        void PostBuild(IServiceProvider provider)
        {
        }

        void PreBuild(IServiceProvider provider)
        {
        }

        #endregion
    }
}