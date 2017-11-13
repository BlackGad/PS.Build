﻿using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Logger = PS.Build.Tasks.Services.Logger;

namespace PS.Build.Tasks
{
    public class SuccessCleanupTask : Task
    {
        #region Override members

        public override bool Execute()
        {
            var logger = new Logger(Log);
            var sandbox = BuildEngine4.GetRegisteredTaskObject(typeof(Sanbox), RegisteredTaskObjectLifetime.Build) as Sanbox;
            if (sandbox == null) return true;

            try
            {
                sandbox.Dispose();
            }
            catch (Exception e)
            {
                logger.Error($"Success cleanup task failed. Details: {e.GetBaseException().Message}");
            }

            return !Log.HasLoggedErrors;
        }

        #endregion
    }
}