using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

                errors.AddRange(preWarnings.AssertContains(0, "Unexpected internal error"));

                errors.AddRange(runner.GetEvents(postBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Custom.AssertEmpty());

                foreach (AttributeTargets value in Enum.GetValues(typeof(AttributeTargets)))
                {
                    if (value == AttributeTargets.All) continue;

                    errors.AddRange(preWarnings.AssertContains(1,
                                                               $"DefinitionLibrary.{value}.EmptyAttribute has no Setup, PreBuid or PostBuild entries. Skipping..."));

                    var expectedCount = 1;
                    if (value == AttributeTargets.Field) expectedCount = 2;

                    errors.AddRange(preMessages.AssertContains(expectedCount, string.Join(",", "PreBuild", value, "PreBuildAttribute")));
                    errors.AddRange(postMessages.AssertContains(expectedCount, string.Join(",", "PostBuild", value, "PostBuildAttribute")));
                    errors.AddRange(preMessages.AssertContains(expectedCount, string.Join(",", "PreBuild", value, "AllAttribute")));
                    errors.AddRange(postMessages.AssertContains(expectedCount, string.Join(",", "PostBuild", value, "AllAttribute")));
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

                errors.AddRange(preWarnings.AssertContains(0, "Unexpected internal error"));

                errors.AddRange(runner.GetEvents(postBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Custom.AssertEmpty());

                foreach (AttributeTargets value in Enum.GetValues(typeof(AttributeTargets)))
                {
                    if (value == AttributeTargets.All) continue;

                    errors.AddRange(preWarnings.AssertContains(1,
                                                               $"DefinitionLibrary.{value}.EmptyAttribute has no Setup, PreBuid or PostBuild entries. Skipping..."));

                    errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "PreBuildAttribute")));
                    errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "PostBuildAttribute")));
                    errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "AllAttribute")));
                    errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "AllAttribute")));
                }
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }

        [Test]
        public void CallOrderingTest()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\GenericProject\");

            var solution = new TestSolution(solutionDirectory);
            var usageLibrary = solution.Project("UsageOrderingLibrary");

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

                errors.AddRange(runner.GetEvents(preBuildTask).Warnings.AssertContains(0, "Unexpected internal error"));
                errors.AddRange(runner.GetEvents(postBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Custom.AssertEmpty());

                var regex = new Regex("^[^,]+,[^,]+,[^,]+,[^,]+,[^,]+$");

                var sequence = new List<AttributeTargets>
                {
                    AttributeTargets.Assembly,
                    AttributeTargets.Field | AttributeTargets.Event, //event as field
                    AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, //param of method
                    AttributeTargets.Constructor | AttributeTargets.Method,
                    AttributeTargets.Property | AttributeTargets.Event, //event as property
                    AttributeTargets.GenericParameter, //param of type definition
                    AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate,
                    AttributeTargets.Module
                };

                var filteredMessages = preMessages.Select(m => m.Message).Where(m => regex.IsMatch(m)).ToList();
                var minIndex = 0;
                for (int i = 0; i < filteredMessages.Count; i++)
                {
                    var message = filteredMessages[i];
                    var index = sequence.Skip(minIndex)
                                        .ToList()
                                        .FindIndex(p => p.HasFlag((AttributeTargets)Enum.Parse(typeof(AttributeTargets), message.Split(',')[1])));
                    if (index == -1) Assert.Fail($"Unexpected message {i}: {message} min({minIndex}:{sequence[minIndex]}) position");
                    minIndex += index;
                }

                filteredMessages = postMessages.Select(m => m.Message).Where(m => regex.IsMatch(m)).ToList();
                minIndex = 0;
                for (int i = 0; i < filteredMessages.Count; i++)
                {
                    var message = filteredMessages[i];
                    var index = sequence.Skip(minIndex)
                                        .ToList()
                                        .FindIndex(p => p.HasFlag((AttributeTargets)Enum.Parse(typeof(AttributeTargets), message.Split(',')[1])));
                    if (index == -1) Assert.Fail($"Unexpected message {i}: {message} min({minIndex}:{sequence[minIndex]}) position");
                    minIndex += index;
                }
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }
    }
}