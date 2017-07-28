using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PS.Build.Services;

namespace DefinitionLibrary.Assembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class AllAttribute : Attribute
    {
        #region Static members

        private static void Process(IEnumerable<string> macroList, IMacroResolver resolver, ILogger logger)
        {
            foreach (var macro in macroList)
            {
                ValidationResult[] errors;
                var result = resolver.Resolve(macro, out errors);
                logger.Info(result);
                if (errors == null) continue;
                foreach (var error in errors)
                {
                    logger.Warn(error.ErrorMessage);
                }
            }
        }

        #endregion

        private readonly List<string> _macroList;

        #region Constructors

        public AllAttribute()
        {
            _macroList = new List<string>
            {
                @"Simple string",
				@"Invalid macro {boo.Platform:2df} string",
                @"{prop.Platform}",
                @"{dir.Project}",
				@"{nuget.Newtonsoft}",
				@"{nuget.Newtonsoft.Json}",
				@"{nuget.Newtonsoft.Json@dir}",
				@"{nuget.Newtonsoft.Json@id}",
				@"{nuget.Newtonsoft.Json@ver}",
				@"{nuget.Newtonsoft.Json@ver:2}",
				@"{env}",
				@"{env.windir}",
				@"{time}",
				@"{time.now}",
				@"{time.now:t}",
            };
        }

        #endregion

        #region Members

        void PostBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var macroResolver = (IMacroResolver)provider.GetService(typeof(IMacroResolver));

            Process(_macroList, macroResolver, logger);
        }

        void PreBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var macroResolver = (IMacroResolver)provider.GetService(typeof(IMacroResolver));

            Process(_macroList, macroResolver, logger);
        }

        #endregion
    }
}