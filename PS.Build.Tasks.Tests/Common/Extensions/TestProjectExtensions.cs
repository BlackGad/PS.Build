using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace PS.Build.Tasks.Tests.Common.Extensions
{
    public static class TestProjectExtensions
    {
        #region Static members

        public static List<ITaskItem> Compile(this TestProject project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            var target = "Rebuild";

            var projectProperties = new Dictionary<string, string>
            {
                { "Configuration", project.Solution.Configuration },
                { "Platform", project.Solution.Platform }
            };

            var result = new List<ITaskItem>();
            var projectToCompile = project.Path;
            var msBuildLogger = new MsBuildLogger
            {
                Verbosity = LoggerVerbosity.Normal
            };

            var pc = new ProjectCollection();

            var buildParameters = new BuildParameters(pc)
            {
                Loggers = new[] { msBuildLogger }
            };

            var projectInstance = new ProjectInstance(projectToCompile, projectProperties, "14.0");
            var buildResults = BuildManager.DefaultBuildManager.Build(buildParameters,
                                                                      new BuildRequestData(projectInstance, new[] { target }));
            TargetResult buildResult;
            if (!buildResults.ResultsByTarget.TryGetValue(target, out buildResult) || buildResult.ResultCode != TargetResultCode.Success)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Project {projectToCompile} compilation failed.");
                foreach (var targetResult in buildResults.ResultsByTarget)
                {
                    builder.AppendLine($"- Target: {targetResult.Key}, Code: ({targetResult.Value.ResultCode})");
                    builder.AppendLine(msBuildLogger.GetLog());
                }

                Assert.Fail(builder.ToString());
            }
            result.AddRange(buildResult.Items.Select(i => new TaskItem(i)));
            return result;
        }

        public static List<ITaskItem> GetReferences(this TestProject project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            var target = "ResolveReferences";

            var projectProperties = new Dictionary<string, string>
            {
                { "Configuration", project.Solution.Configuration },
                { "Platform", project.Solution.Platform }
            };

            var result = new List<ITaskItem>();
            var projectToCompile = project.Path;
            var msBuildLogger = new MsBuildLogger
            {
                Verbosity = LoggerVerbosity.Normal
            };

            var pc = new ProjectCollection();

            var buildParameters = new BuildParameters(pc)
            {
                Loggers = new[] { msBuildLogger }
            };

            var projectInstance = new ProjectInstance(projectToCompile, projectProperties, "14.0");
            var buildResults = BuildManager.DefaultBuildManager.Build(buildParameters,
                                                                      new BuildRequestData(projectInstance,
                                                                                           new[] { target },
                                                                                           null,
                                                                                           BuildRequestDataFlags.ProvideProjectStateAfterBuild));
            TargetResult buildResult;
            if (!buildResults.ResultsByTarget.TryGetValue(target, out buildResult) || buildResult.ResultCode != TargetResultCode.Success)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Project {projectToCompile} compilation failed.");
                foreach (var targetResult in buildResults.ResultsByTarget)
                {
                    builder.AppendLine($"- Target: {targetResult.Key}, Code: ({targetResult.Value.ResultCode})");
                    builder.AppendLine(msBuildLogger.GetLog());
                }

                Assert.Fail(builder.ToString());
            }
            result.AddRange(buildResults.ProjectStateAfterBuild.GetItems("ReferencePath").Select(i => new TaskItem(i)));
            return result;
        }

        #endregion
    }
}