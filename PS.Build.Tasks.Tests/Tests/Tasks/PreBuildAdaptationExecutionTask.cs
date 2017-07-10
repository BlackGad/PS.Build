﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using PS.Build.Tasks.Extensions;
using PS.Build.Tasks.Tests.Common;

namespace PS.Build.Tasks.Tests.Tasks
{
    [TestFixture]
    class PreBuildAdaptationExecutionTaskTests
    {
        [Test]
        public void Test()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\GenericProject\");
            var definitionProjectPath = Path.Combine(solutionDirectory, @"DefinitionLibrary\DefinitionLibrary.csproj");
            var usageProjectPath = Path.Combine(solutionDirectory, @"UsageLibrary\UsageLibrary.csproj");
            var usageProjectDirectory = Path.GetDirectoryName(usageProjectPath);
            Assert.IsNotNull(usageProjectDirectory, $"{nameof(usageProjectDirectory)} is invalid");

            var configuration = "Release";
            var platform = "AnyCPU";

            var xProject = XDocument.Load(usageProjectPath);
            Assert.IsNotNull(xProject, $"{nameof(xProject)} was not loaded from {usageProjectPath}");

            var msbuildNS = "http://schemas.microsoft.com/developer/msbuild/2003";
            var outputPath = xProject.Descendants(XName.Get("OutputPath", msbuildNS))
                                     .FirstOrDefault(e => e.Parent?.Attributes().Any(a => a.Value.Contains(configuration)) == true)?.Value;

            Assert.IsNotNull(outputPath, $"{nameof(outputPath)} was not found in {usageProjectPath} project file");

            var pc = new ProjectCollection();

            var projectProperties = new Dictionary<string, string>
            {
                { "Configuration", configuration },
                { "Platform", platform }
            };

            var target = "Rebuild";
            var buildParameters = new BuildParameters(pc);

            var msBuildLogger = new MsBuildLogger
            {
                Verbosity = LoggerVerbosity.Normal
            };
            buildParameters.Loggers = new[] { msBuildLogger };
            var projectInstance = new ProjectInstance(definitionProjectPath, projectProperties, "14.0");
            var result = BuildManager.DefaultBuildManager.Build(buildParameters,
                                                                new BuildRequestData(projectInstance, new[] { target }));
            TargetResult buildResult;
            if (!result.ResultsByTarget.TryGetValue(target, out buildResult) || buildResult.ResultCode != TargetResultCode.Success)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Project {definitionProjectPath} compilation failed.");
                foreach (var targetResult in result.ResultsByTarget)
                {
                    builder.AppendLine($"- Target: {targetResult.Key}, Code: ({targetResult.Value.ResultCode})");
                    builder.AppendLine(msBuildLogger.GetLog());
                }

                Assert.Fail(builder.ToString());
            }

            var taskObjectTable = new Dictionary<object, object>();
            var preBuildTask = BuildEngineRunner.Create<PreBuildAdaptationExecutionTask>(usageProjectPath,
                                                                                         taskObjectTable,
                                                                                         ev => { },
                                                                                         ev => { });

            preBuildTask.References = buildResult.Items;

            preBuildTask.PropertyProjectFile = usageProjectPath;
            preBuildTask.PropertyPlatform = platform;
            preBuildTask.PropertyConfiguration = configuration;
            preBuildTask.PropertyRootNamespace = xProject.Descendants(XName.Get("RootNamespace", msbuildNS)).FirstOrDefault()?.Value;

            preBuildTask.DirectoryIntermediate = "obj\\" + configuration;
            preBuildTask.DirectoryProject = usageProjectDirectory;
            preBuildTask.DirectoryTarget = Path.Combine(usageProjectDirectory, outputPath);
            preBuildTask.DirectorySolution = solutionDirectory;

            foreach (var pair in PreBuildAdaptationExecutionTask.ItemsProperties)
            {
                if (pair.Value == null) continue;

                var items = xProject.Descendants(XName.Get(pair.Key.ToString(), msbuildNS))
                                    .Select(n =>
                                    {
                                        var relativePath = n.Attribute(XName.Get("Include"))?.Value ?? string.Empty;
                                        return new TaskItem(Path.Combine(usageProjectDirectory, relativePath));
                                    })
                                    .Enumerate<ITaskItem>()
                                    .ToArray();

                pair.Value.SetValue(preBuildTask, items);
            }

            preBuildTask.Execute();

            var postBuildTask = BuildEngineRunner.Create<PostBuildAdaptationExecutionTask>(usageProjectPath,
                                                                                           taskObjectTable,
                                                                                           ev => { },
                                                                                           ev => { });
            postBuildTask.Execute();

            foreach (var instance in taskObjectTable.Values)
            {
                var disposable = instance as IDisposable;
                disposable?.Dispose();
            }
        }
    }
}