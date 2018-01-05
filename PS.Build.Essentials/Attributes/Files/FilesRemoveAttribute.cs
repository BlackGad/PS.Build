using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PS.Build.Extensions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class FilesRemoveAttribute : FilesBaseAttribute
    {
        #region Constructors

        public FilesRemoveAttribute(string selectPattern, params string[] filterPatterns) :
            base(selectPattern, filterPatterns)
        {
            RemoveEmptyDirectories = true;
        }

        #endregion

        #region Properties

        public bool RemoveEmptyDirectories { get; set; }

        #endregion

        #region Override members

        protected override void Process(RecursivePath[] files, IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            logger.Info(files.Any() ? $"There is {files.Length} files to remove:" : "There is no files to remove");
            Debugger.Launch();
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file.Original);
                    logger.Info($"* Removed: {file.Original}");
                    if (RemoveEmptyDirectories) file.RemoveEmptyDirectories();
                }
                catch (Exception e)
                {
                    logger.Warn($"Cannot remove {file.Original} file. Details: {e.GetBaseException().Message}");
                }
            }
        }

        

        #endregion
    }
}