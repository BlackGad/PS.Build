using System;
using System.ComponentModel;
using System.Reflection;
using PS.Build.Services;

namespace DefinitionLibrary.Class
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PreBuildAttribute : Attribute
    {
        #region Members

        void PreBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var type = GetType();
            var validOn = type.GetCustomAttribute<AttributeUsageAttribute>().ValidOn;
            logger.Info(string.Join(",", "PreBuild", validOn, type.Name));
        }

        #endregion
    }
}