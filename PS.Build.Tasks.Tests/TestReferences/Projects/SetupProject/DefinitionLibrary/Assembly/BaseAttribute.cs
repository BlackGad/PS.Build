using System;
using PS.Build.Services;

namespace DefinitionLibrary.Assembly
{
    public abstract class BaseAttribute : Attribute
    {
        #region Static members

        private static void Setup(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            logger.Info("@Setup base");
        }

        #endregion
    }
}