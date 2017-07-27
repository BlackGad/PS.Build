using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PS.Build.Tasks.Tests.Common;
using PS.Build.Tasks.Tests.Common.Extensions;

namespace PS.Build.Tasks.Tests.Tasks
{
    [TestFixture]
    public class DynamicVaultTests
    {
        [Test]
        public void AttributeUsageTest()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\DynamicVaultProject\");

            var solution = new TestSolution(solutionDirectory);
            var usageLibrary = solution.Project("UsageLibrary");

            var errors = new List<string>();
            using (var runner = new BuildEngineRunner(usageLibrary.Path))
            {
                var preBuildTask = runner.Create<PreBuildAdaptationExecutionTask>();
                preBuildTask.Setup(usageLibrary);

                preBuildTask.References = usageLibrary.GetReferences().ToArray();
                preBuildTask.Execute();

                var postBuildTask = runner.Create<PostBuildAdaptationExecutionTask>();
                postBuildTask.Execute();

                errors.AddRange(runner.GetEvents(preBuildTask).Messages.AssertContains(1, string.Join(",", "Stored", Guid.Empty)));
                errors.AddRange(runner.GetEvents(postBuildTask).Messages.AssertContains(1, string.Join(",", "Received", Guid.Empty)));

                errors.AddRange(runner.GetEvents(preBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Custom.AssertEmpty());
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }
    }
}