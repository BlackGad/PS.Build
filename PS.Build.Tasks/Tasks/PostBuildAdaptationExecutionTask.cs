using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Logger = PS.Build.Tasks.Services.Logger;

namespace PS.Build.Tasks
{
    public class PostBuildAdaptationExecutionTask : Task
    {
        #region Override members

        public override bool Execute()
        {
            var logger = new Logger(Log);
            var sandbox = BuildEngine4.GetRegisteredTaskObject(typeof(Sanbox), RegisteredTaskObjectLifetime.Build) as Sanbox;
            if (sandbox == null) return true;

            try
            {
                sandbox.Client.ExecutePostBuildAdaptations(logger);
            }
            catch (Exception e)
            {
                logger.Error($"Assembly adaptation pre build execution failed. Details: {e.GetBaseException().Message}");
            }

            return !Log.HasLoggedErrors;
        }

        #endregion
    }
}