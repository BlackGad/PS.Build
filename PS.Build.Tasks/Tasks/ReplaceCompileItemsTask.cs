using System;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Logger = PS.Build.Tasks.Services.Logger;

namespace PS.Build.Tasks
{
    public class ReplaceCompileItemsTask : Task
    {
        #region Properties

        public bool Test { get; set; }

        [Output]
        public ITaskItem[] CompilesToAdd { get; private set; }

        [Output]
        public ITaskItem[] CompilesToRemove { get; private set; }

        [Required]
        public ITaskItem[] ItemsCompile { get; set; }

        #endregion

        #region Override members

        public override bool Execute()
        {
            var logger = new Logger(Log);

            var sandbox = BuildEngine4.GetRegisteredTaskObject(BuildEngine.ProjectFileOfTaskNode, RegisteredTaskObjectLifetime.Build) as Sanbox;
            if (sandbox == null) return true;

            CompilesToAdd = Enumerable.Empty<ITaskItem>().ToArray();
            CompilesToRemove = Enumerable.Empty<ITaskItem>().ToArray();
            if (!Test && Assembly.GetEntryAssembly() == null) return true;

            try
            {
                var replacements = sandbox.Client.ReplaceCompileItems(logger);
                Func<CompileItemReplacement, ITaskItem> selector = r =>
                {
                    return ItemsCompile.FirstOrDefault(c => string.Equals(c.GetMetadata("FullPath"),
                                                                          r.Source,
                                                                          StringComparison.InvariantCultureIgnoreCase));
                };

                CompilesToAdd = replacements.Select(r => new TaskItem(r.Target)).OfType<ITaskItem>().ToArray();
                CompilesToRemove = replacements.Select(selector).ToArray();
            }
            catch (Exception e)
            {
                logger.Error($"Assembly compile replacement failed. Details: {e.GetBaseException().Message}");
            }

            return !Log.HasLoggedErrors;
        }

        #endregion
    }
}