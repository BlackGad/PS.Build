using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Delegate
{
    [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PreBuildAttribute : BaseAttribute
    {
        #region Constructors

        public PreBuildAttribute([CallerFilePath] string file = null) : base(file)
        {
        }

        #endregion

        #region Members

        void PreBuild(IServiceProvider provider)
        {
            BasePreBuild(provider);
        }

        #endregion
    }
}