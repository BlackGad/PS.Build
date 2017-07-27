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
    class BuildAdaptationExecutionTaskTests
    {
        [Test]
        public void AttributeUsageTest()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\GenericProject\");

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

                var preMessages = runner.GetEvents(preBuildTask).Messages;
                var postMessages = runner.GetEvents(postBuildTask).Messages;
                var preWarnings = runner.GetEvents(preBuildTask).Warnings;

                errors.AddRange(runner.GetEvents(postBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Custom.AssertEmpty());

                foreach (AttributeTargets value in Enum.GetValues(typeof(AttributeTargets)))
                {
                    if (value == AttributeTargets.All) continue;

                    errors.AddRange(preWarnings.AssertContains(1,
                                                               $"DefinitionLibrary.{value}.EmptyAttribute has no PreBuid or PostBuild entries. Skipping..."));

                    errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "PreBuildAttribute")));
                    errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "PostBuildAttribute")));
                    errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "AllAttribute")));
                    errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "AllAttribute")));
                }
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void AttributeUsageWithDirectivesTest()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\GenericProject\");

            var solution = new TestSolution(solutionDirectory);

            var usageLibrary = solution.Project("UsageWithDirectivesLibrary");
            var errors = new List<string>();
            using (var runner = new BuildEngineRunner(usageLibrary.Path))
            {
                var preBuildTask = runner.Create<PreBuildAdaptationExecutionTask>();
                preBuildTask.Setup(usageLibrary);

                preBuildTask.References = usageLibrary.GetReferences().ToArray();
                preBuildTask.Execute();

                var postBuildTask = runner.Create<PostBuildAdaptationExecutionTask>();
                postBuildTask.Execute();

                var preMessages = runner.GetEvents(preBuildTask).Messages;
                var postMessages = runner.GetEvents(postBuildTask).Messages;
                var preWarnings = runner.GetEvents(preBuildTask).Warnings;

                errors.AddRange(runner.GetEvents(postBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Custom.AssertEmpty());

                foreach (AttributeTargets value in Enum.GetValues(typeof(AttributeTargets)))
                {
                    if (value == AttributeTargets.All) continue;

                    errors.AddRange(preWarnings.AssertContains(1,
                                                               $"DefinitionLibrary.{value}.EmptyAttribute has no PreBuid or PostBuild entries. Skipping..."));

                    errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "PreBuildAttribute")));
                    errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "PostBuildAttribute")));
                    errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "AllAttribute")));
                    errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "AllAttribute")));
                }
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }
    }
}