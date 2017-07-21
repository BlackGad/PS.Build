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
        //private static IEnumerable<string> CheckEntry(IEnumerable<string> availableMessages, string message)
        //{
        //    var count = availableMessages.Count(m => Equals(m, message));
        //    if (count != 1) return new[] { $"Expected message '{message}' raised {count} times" };
        //    return Enumerable.Empty<string>();
        //}

        [Test]
        public void GenericAttributeUsageTest()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\GenericProject\");

            var solution = new TestSolution(solutionDirectory);

            var references = solution.Project("DefinitionLibrary").Compile();
            var usageLibrary = solution.Project("UsageLibrary");
            var errors = new List<string>();
            using (var runner = new BuildEngineRunner(usageLibrary.Path))
            {
                var preBuildTask = runner.Create<PreBuildAdaptationExecutionTask>();
                preBuildTask.Setup(usageLibrary);

                preBuildTask.References = references.ToArray();
                preBuildTask.Execute();

                var postBuildTask = runner.Create<PostBuildAdaptationExecutionTask>();
                postBuildTask.Execute();

                var preMessages = runner.GetEvents(preBuildTask).Messages.Select(m => m.Message).ToList();
                var postMessages = runner.GetEvents(postBuildTask).Messages.Select(m => m.Message).ToList();
                var preWarnings = runner.GetEvents(preBuildTask).Warnings.Select(m => m.Message).ToList();
                var postWarnings = runner.GetEvents(postBuildTask).Warnings.Select(m => m.Message).ToList();

                var preErrors = runner.GetEvents(preBuildTask).Errors.Select(m => m.Message).ToList();
                if (preErrors.Any()) Assert.Fail(string.Join(Environment.NewLine, preErrors));

                var postErrors = runner.GetEvents(postBuildTask).Errors.Select(m => m.Message).ToList();
                if (postErrors.Any()) Assert.Fail(string.Join(Environment.NewLine, postErrors));

                var preCustom = runner.GetEvents(preBuildTask).Custom.Select(m => m.Message).ToList();
                if (preCustom.Any()) Assert.Fail(string.Join(Environment.NewLine, preCustom));

                var postCustom = runner.GetEvents(postBuildTask).Custom.Select(m => m.Message).ToList();
                if (postCustom.Any()) Assert.Fail(string.Join(Environment.NewLine, postCustom));

                foreach (AttributeTargets value in Enum.GetValues(typeof(AttributeTargets)))
                {
                    if (value == AttributeTargets.All) continue;

                    errors.AddRange(preWarnings.AssertContains(1, $"DefinitionLibrary.{value}.EmptyAttribute has no PreBuid or PostBuild entries. Skipping..."));

                    //Skipping not supported attribute usage
                    if (value != AttributeTargets.ReturnValue)
                    {
                        errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "PreBuildAttribute")));
                        errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "PostBuildAttribute")));
                        errors.AddRange(preMessages.AssertContains(1, string.Join(",", "PreBuild", value, "AllAttribute")));
                        errors.AddRange(postMessages.AssertContains(1, string.Join(",", "PostBuild", value, "AllAttribute")));
                    }
                    else
                    {
                        errors.AddRange(preMessages.AssertContains(3,
                                                                   "Not supported feature. Details: Cannot resolve attribute data",
                                                                   "DefinitionLibrary.ReturnValue"));
                    }
                }
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }
    }
}