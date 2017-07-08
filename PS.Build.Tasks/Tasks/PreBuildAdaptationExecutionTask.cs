using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PS.Build.Types;
using Logger = PS.Build.Tasks.Services.Logger;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace PS.Build.Tasks
{
    public class PreBuildAdaptationExecutionTask : Task
    {
        #region Constants

        private static readonly Dictionary<BuildItem, PropertyInfo> ArtifactsProperties;

        #endregion

        #region Constructors

        static PreBuildAdaptationExecutionTask()
        {
            var type = typeof(PreBuildAdaptationExecutionTask);
            ArtifactsProperties = Enum.GetValues(typeof(BuildItem))
                                      .OfType<BuildItem>()
                                      .ToDictionary(v => v, v => type.GetProperty($"Artifacts{v}"));
        }

        #endregion

        #region Properties

        [Output]
        public ITaskItem[] ArtifactsAdditionalFiles { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsCompile { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsContent { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsEmbeddedResource { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsEntityDeploy { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsNone { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsPage { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsResource { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsTemporary { get; private set; }

        #endregion

        #region Override members

        public override bool Execute()
        {
            var logger = new Logger(Log);
            var sandbox = BuildEngine4.GetRegisteredTaskObject(typeof(Sanbox), RegisteredTaskObjectLifetime.Build) as Sanbox;
            if (sandbox == null) return true;

            try
            {
                var artifacts = sandbox.Client.ExecutePreBuildAdaptations(logger);

                if (artifacts.Any())
                {
                    logger.Debug("------------");
                    logger.Info("Adding additional TaskItems to build");

                    foreach (var builder in artifacts)
                    {
                        logger.Info($"+ {builder}");
                    }

                    FillOutputTaskItems(artifacts);
                }

                ArtifactsTemporary = artifacts.Where(a => !a.IsPermanent)
                                              .Select(a => new TaskItem(a.Path))
                                              .OfType<ITaskItem>()
                                              .ToArray();
            }
            catch (Exception e)
            {
                logger.Error($"Assembly adaptation pre build execution failed. Details: {e.GetBaseException().Message}");
            }

            return !Log.HasLoggedErrors;
        }

        #endregion

        #region Members

        private void FillOutputTaskItems(SerializableArtifact[] artifacts)
        {
            var groupedArtifacts = artifacts.ToLookup(b => b.Type, b => b);
            foreach (var group in groupedArtifacts)
            {
                if (!ArtifactsProperties.ContainsKey(group.Key)) continue;
                if (ArtifactsProperties[group.Key] == null) continue;
                ArtifactsProperties[group.Key].SetValue(this, group.Select(b => new TaskItem(b.Path, b.Metadata)).ToArray());
            }
        }

        #endregion
    }
}