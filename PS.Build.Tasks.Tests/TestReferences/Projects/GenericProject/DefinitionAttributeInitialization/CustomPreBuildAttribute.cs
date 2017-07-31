using System;
using System.ComponentModel;

namespace DefinitionAttributeInitialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class CustomPreBuildAttribute : BaseCustomAttribute
    {
        #region Constructors

        public CustomPreBuildAttribute(uint aaaaaa, int a = 0, string b = null, float c = 0, string def = null, params byte[] ssss)
        {
        }

        public CustomPreBuildAttribute()
        {
        }

        public CustomPreBuildAttribute(uint aaaaaa, int a = 0)
        {
        }

        #endregion

        #region Properties

        public double D { get; set; }
        public Double D2 { get; set; }

        #endregion

        #region Members

        void PreBuild(IServiceProvider provider)
        {
        }

        #endregion
    }
}