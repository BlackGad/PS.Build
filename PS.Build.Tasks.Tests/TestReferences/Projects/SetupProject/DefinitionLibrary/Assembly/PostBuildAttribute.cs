using System;
using System.ComponentModel;
using PS.Build.Services;

namespace DefinitionLibrary.Assembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PostBuildAttribute : BaseAttribute
    {
        #region Static members

        private static void Setup(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            logger.Info($"@Setup {nameof(PostBuildAttribute)}");
        }

        #endregion

        #region Members

        void PostBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            logger.Info($"@{nameof(PostBuildAttribute)}");
        }

        #endregion
    }
}