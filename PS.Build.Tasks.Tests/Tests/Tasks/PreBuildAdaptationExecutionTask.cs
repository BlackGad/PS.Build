using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Assert.IsNotNull(usageProjectDirectory);

            var configuration = "Release";
            var platform = "AnyCPU";

            var xProject = XDocument.Load(usageProjectPath);
            Assert.IsNotNull(xProject);

            var msbuildNS = "http://schemas.microsoft.com/developer/msbuild/2003";
            var outputPath = xProject.Descendants(XName.Get("OutputPath", msbuildNS))
                                     .FirstOrDefault(e => e.Parent?.Attributes().Any(a => a.Value.Contains(configuration)) == true)?.Value;

            Assert.IsNotNull(outputPath);

            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", configuration);
            pc.SetGlobalProperty("Platform", "Any CPU");
            var target = "Rebuild";
            var result = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc),
                                                                new BuildRequestData(new ProjectInstance(definitionProjectPath),
                                                                                     new[] { target }));
            TargetResult buildResult;
            Assert.IsTrue(result.ResultsByTarget.TryGetValue(target, out buildResult));
            Assert.AreEqual(TargetResultCode.Success, buildResult.ResultCode);
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