using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using PS.Build.Extensions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FilesEmbedAttribute : FilesBaseAttribute
    {
        #region Constructors

        public FilesEmbedAttribute(string selectPattern, params string[] filterPatterns) :
            base(selectPattern, filterPatterns)
        {
            BuildStep = BuildStep.PreBuild;
        }

        #endregion

        #region Properties

        public string NamePrefix { get; set; }

        #endregion

        #region Override members

        protected override void Process(RecursivePath[] files, IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            if (BuildStep.HasFlag(BuildStep.PostBuild))
            {
                logger.Error("Files could not be embedded on PostBuild event");
                return;
            }

            var macroResolver = provider.GetService<IMacroResolver>();
            var artifactory = provider.GetService<IArtifactory>();
            var namePrefix = macroResolver.Resolve(NamePrefix ?? string.Empty);
            logger.Info(files.Any() ? $"There is {files.Length} files to embed:" : "There is no files to embed");

            foreach (var file in files)
            {
                var artifact = artifactory.Artifact(file.Original, BuildItem.EmbeddedResource)
                                          .Permanent();

                if (!string.IsNullOrWhiteSpace(namePrefix) && !string.IsNullOrWhiteSpace(file.Recursive))
                {
                    artifact.Metadata("Link", Path.Combine(namePrefix, file.Recursive, file.Postfix));
                }

                artifact.Dependencies().FileDependency(file.Original);
            }
        }

        #endregion
    }
}