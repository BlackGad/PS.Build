using System;
using System.ComponentModel;
using PS.Build.Services;

namespace DefinitionLibrary.Assembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PreBuildAttribute : BaseAttribute
    {
        #region Static members

        private static void Setup(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            logger.Info($"@Setup {nameof(PreBuildAttribute)}");
        }

        #endregion

        #region Members

        void PreBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            logger.Info($"@{nameof(PreBuildAttribute)}");
        }

        #endregion
    }
}