using System;
using System.ComponentModel;
using System.Reflection;
using PS.Build.Services;

namespace DefinitionLibrary.Field
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class PostBuildAttribute : Attribute
    {
        #region Members

        void PostBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var type = GetType();
            var validOn = type.GetCustomAttribute<AttributeUsageAttribute>().ValidOn;
            logger.Info(string.Join(",", "PostBuild", validOn, type.Name));
        }

        #endregion
    }
}