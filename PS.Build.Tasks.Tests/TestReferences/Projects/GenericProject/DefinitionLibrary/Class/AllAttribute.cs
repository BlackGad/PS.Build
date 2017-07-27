using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Class
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [Designer("PS.Build.Adaptation")]
    public sealed class AllAttribute : BaseAttribute
    {
        #region Constructors

        public AllAttribute([CallerLineNumber] int position = default(int), [CallerFilePath] string file = null) : base(position, file)
        {
        }

        #endregion

        #region Members

        void PostBuild(IServiceProvider provider)
        {
            BasePostBuild(provider);
        }

        void PreBuild(IServiceProvider provider)
        {
            BasePreBuild(provider);
        }

        #endregion
    }
}