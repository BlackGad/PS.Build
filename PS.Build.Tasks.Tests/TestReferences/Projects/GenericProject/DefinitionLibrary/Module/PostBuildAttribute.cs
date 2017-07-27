using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Module
{
    [AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PostBuildAttribute : BaseAttribute
    {
        #region Constructors

        public PostBuildAttribute([CallerLineNumber] int position = default(int), [CallerFilePath] string file = null) : base(position, file)
        {
        }

        #endregion

        #region Members

        void PostBuild(IServiceProvider provider)
        {
            BasePostBuild(provider);
        }

        #endregion
    }
}