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
    public class FilesCopyAttribute : FilesBaseAttribute
    {
        #region Constructors

        public FilesCopyAttribute(string selectPattern, params string[] filterPatterns) :
            base(selectPattern, filterPatterns)
        {
            OverwriteExisting = true;
        }

        #endregion

        #region Properties

        public bool OverwriteExisting { get; set; }

        public string TargetFolder { get; set; }

        #endregion

        #region Override members

        protected override void Process(RecursivePath[] files, IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var macroResolver = provider.GetService<IMacroResolver>();
            var targetFolder = macroResolver.Resolve(TargetFolder);
            var explorer = provider.GetService<IExplorer>();

            if (string.IsNullOrWhiteSpace(targetFolder)) targetFolder = explorer.Directories[BuildDirectory.Target];

            logger.Info(files.Any() ? $"There is {files.Length} files to copy:" : "There is no files to copy");

            foreach (var file in files)
            {
                var targetFile = Path.Combine(targetFolder, file.Recursive, file.Postfix);
                try
                {
                    Path.GetDirectoryName(targetFile).EnsureDirectoryExist();
                    File.Copy(file.Original, targetFile, OverwriteExisting);
                    logger.Info($"* Copied: {file.Original} -> {targetFile}");
                }
                catch (Exception e)
                {
                    logger.Warn($"Cannot copy {file.Original} file to {targetFile}. Details: {e.GetBaseException().Message}");
                }
            }
        }

        #endregion
    }
}