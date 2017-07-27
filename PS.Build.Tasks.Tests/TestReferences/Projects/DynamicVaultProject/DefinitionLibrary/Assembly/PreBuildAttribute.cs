using System;
using System.ComponentModel;
using PS.Build.Services;

namespace DefinitionLibrary.Assembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PreBuildAttribute : Attribute
    {
        #region Members

        void PreBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var vault = (IDynamicVault)provider.GetService(typeof(IDynamicVault));

            var value = vault.Store(typeof(Guid), Guid.Empty);
            logger.Info(string.Join(",", "Stored", value));
        }

        #endregion
    }
}