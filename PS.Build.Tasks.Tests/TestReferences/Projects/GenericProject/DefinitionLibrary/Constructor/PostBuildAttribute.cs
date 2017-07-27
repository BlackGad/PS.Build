using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Constructor
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PostBuildAttribute : BaseAttribute
    {
        #region Constructors

        public PostBuildAttribute([CallerFilePath] string file = null) : base(file)
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