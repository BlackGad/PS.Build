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
    public class MacroResolverTests
    {
        private IEnumerable<string> CheckTaskEvents(string solutionDirectory, TaskEvents taskEvents)
        {
            var errors = new List<string>();
            var preMessages = taskEvents.Messages
                                        .SkipWhile(m => !Equals(m.Message, "Adaptation: DefinitionLibrary.Assembly.AllAttribute"))
                                        .TakeWhile(m => !Equals(m.Message, "------------"))
                                        .ToList();
            errors.AddRange(preMessages.AssertContains(1, @"|Simple string|"));
            errors.AddRange(preMessages.AssertContains(1, @"|AnyCPU|"));
            errors.AddRange(preMessages.AssertContains(1, $@"|{solutionDirectory}UsageLibrary\|"));
            errors.AddRange(preMessages.AssertContains(2, $@"|{solutionDirectory}packages\Newtonsoft.Json.10.0.3\|"));
            errors.AddRange(preMessages.AssertContains(1, @"|Newtonsoft.Json|"));
            errors.AddRange(preMessages.AssertContains(1, @"|10.0.3.0|"));
            errors.AddRange(preMessages.AssertContains(1, @"|10.0|"));
            errors.AddRange(preMessages.AssertContains(1, "|{env}|"));
            errors.AddRange(preMessages.AssertContains(1, "|{nuget.Newtonsoft}|"));
            errors.AddRange(preMessages.AssertContains(2, "|" + Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "|"));

            var preWarnings = taskEvents.Warnings;
            errors.AddRange(preWarnings.AssertContains(1, "{boo.Platform:2df}"));
            errors.AddRange(preWarnings.AssertContains(1, "Package 'Newtonsoft' not found"));
            errors.AddRange(preWarnings.AssertContains(1, "Illegal environment variable"));
            errors.AddRange(preWarnings.AssertContains(1, "Invalid time option"));
            errors.AddRange(preWarnings.AssertContains(1, "Invalid uid option"));
            errors.AddRange(preWarnings.AssertContains(1, "Invalid SpecialFolder option"));
            errors.AddRange(preWarnings.AssertContains(1, "Not supported 'notexisted' folder"));
            Assert.AreEqual(7, preWarnings.Count);

            errors.AddRange(taskEvents.Errors.AssertEmpty());
            errors.AddRange(taskEvents.Custom.AssertEmpty());

            return errors;
        }

        [Test]
        public void Test()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\MacroResolverProject\");

            var solution = new TestSolution(solutionDirectory);
            solution.RestoreNuget();

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
                errors.AddRange(CheckTaskEvents(solutionDirectory, runner.GetEvents(preBuildTask)));
                errors.AddRange(CheckTaskEvents(solutionDirectory, runner.GetEvents(postBuildTask)));
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }
    }
}