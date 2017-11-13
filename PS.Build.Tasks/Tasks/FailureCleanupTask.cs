using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using PS.Build.Tasks.Services;
using Task = Microsoft.Build.Utilities.Task;

namespace PS.Build.Tasks
{
    public class FailureCleanupTask : Task
    {
        public override bool Execute()
        {
            var logger = new Logger(Log);

            var sandbox = BuildEngine4.GetRegisteredTaskObject(BuildEngine.ProjectFileOfTaskNode, RegisteredTaskObjectLifetime.Build) as Sanbox;
            if (sandbox == null) return true;

            try
            {
                sandbox.Dispose();
                BuildEngine4.UnregisterTaskObject(BuildEngine.ProjectFileOfTaskNode, RegisteredTaskObjectLifetime.Build);
            }
            catch (Exception e)
            {
                logger.Error($"Failure cleanup task failed. Details: {e.GetBaseException().Message}");
            }

            return !Log.HasLoggedErrors;
        }
    }
}
