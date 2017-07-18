using System;
using System.ComponentModel;
using System.Reflection;
using PS.Build.Services;

namespace DefinitionLibrary.Interface
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class AllAttribute : Attribute
    {
        #region Members

        void PostBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var type = GetType();
            var validOn = type.GetCustomAttribute<AttributeUsageAttribute>().ValidOn;
            logger.Info(string.Join(",", "PostBuild", validOn, type.Name));
        }

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