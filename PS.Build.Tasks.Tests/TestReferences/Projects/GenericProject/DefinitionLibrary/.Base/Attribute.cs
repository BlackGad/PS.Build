using System;
using System.IO;
using System.Reflection;
using PS.Build.Services;

namespace DefinitionLibrary
{
    public abstract class BaseAttribute : Attribute
    {
        private readonly string _file;
        private readonly int _line;

        #region Constructors

        protected BaseAttribute(int line, string file)
        {
            _line = line;
            _file = Path.GetFileName(file);
        }

        #endregion

        #region Members

        protected void BasePostBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var type = GetType();
            var validOn = type.GetCustomAttribute<AttributeUsageAttribute>().ValidOn;
            logger.Info(string.Join(",", "PostBuild", validOn, type.Name, _line, _file));
        }

        protected void BasePreBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var type = GetType();
            var validOn = type.GetCustomAttribute<AttributeUsageAttribute>().ValidOn;
            logger.Info(string.Join(",", "PreBuild", validOn, type.Name, _line, _file));
        }

        #endregion
    }
}