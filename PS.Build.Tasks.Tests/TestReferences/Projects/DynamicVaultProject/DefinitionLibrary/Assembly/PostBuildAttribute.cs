using System;
using System.ComponentModel;
using PS.Build.Services;

namespace DefinitionLibrary.Assembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PostBuildAttribute : Attribute
    {
        #region Members

        void PostBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var vault = (IDynamicVault)provider.GetService(typeof(IDynamicVault));

            object value;
            if (vault.Query(typeof(Guid), out value)) logger.Info(string.Join(",", "Received", value));
            else logger.Error("Value not found");
        }

        #endregion
    }
}