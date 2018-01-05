using System;
using System.Collections.Generic;
using System.Linq;
using PS.Build.Extensions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public abstract class FilesBaseAttribute : Attribute
    {
        #region Constructors

        protected FilesBaseAttribute(string selectPattern, string[] filterPatterns)
        {
            SelectPattern = selectPattern;
            FilterPatterns = filterPatterns;
            BuildStep = BuildStep.PostBuild;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether to start file operations.
        /// </summary>
        public BuildStep BuildStep { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether adaptation fails if file list is empty
        /// </summary>
        public bool FailOnEmpty { get; set; }

        public string[] FilterPatterns { get; }

        public string SelectPattern { get; }

        #endregion

        #region Members

        protected virtual RecursivePath[] EnumerateFiles(IServiceProvider provider, string selectPattern, string[] filterPatterns)
        {
            var foundItems = IOExtensions.SearchFiles(selectPattern, (pattern, file) => file.DetermineRecursivePath(pattern))
                                         .Items
                                         .ToList();

            var filteredFiles = new List<RecursivePath>();
            var logger = provider.GetService<ILogger>();

            foreach (var pattern in filterPatterns)
            {
                var paths = IOExtensions.Match(foundItems, pattern, path => path.Original).ToList();

                if (paths.Any())
                {
                    logger.Info($"Files filtered by {pattern} pattern:");
                    foreach (var path in paths)
                    {
                        logger.Info($"- {path.Original}");
                    }
                }
                else
                {
                    logger.Info($"There is no files to filter by {pattern} pattern");
                }

                filteredFiles.AddRange(paths);
            }

            filteredFiles = filteredFiles.Distinct().ToList();
            return foundItems.Except(filteredFiles).ToArray();
        }

        protected abstract void Process(RecursivePath[] files, IServiceProvider provider);

        private void PostBuild(IServiceProvider provider)
        {
            if (BuildStep.HasFlag(BuildStep.PostBuild))
            {
                var macroResolver = provider.GetService<IMacroResolver>();
                var selectPattern = macroResolver.Resolve(SelectPattern);
                var filterPatterns = FilterPatterns.Enumerate()
                                                   .Select(f => macroResolver.Resolve(f))
                                                   .ToArray();

                var files = EnumerateFiles(provider, selectPattern, filterPatterns);
                if (!files.Any() && FailOnEmpty)
                    provider.GetService<ILogger>().Error("There is no available files to process");
                Process(files, provider);
            }
        }

        private void PreBuild(IServiceProvider provider)
        {
            if (BuildStep.HasFlag(BuildStep.PreBuild))
            {
                var macroResolver = provider.GetService<IMacroResolver>();
                var selectPattern = macroResolver.Resolve(SelectPattern);
                var filterPatterns = FilterPatterns.Enumerate()
                                                   .Select(f => macroResolver.Resolve(f))
                                                   .ToArray();
                var files = EnumerateFiles(provider, selectPattern, filterPatterns);
                if (!files.Any() && FailOnEmpty)
                    provider.GetService<ILogger>().Error("There is no available files to process");
                Process(files, provider);
            }
        }

        #endregion
    }
}