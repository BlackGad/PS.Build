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
        [Test]
        public void AttributeUsageTest()
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

                var preMessages = runner.GetEvents(preBuildTask)
                                        .Messages
                                        .SkipWhile(m => !Equals(m.Message, "Adaptation: DefinitionLibrary.Assembly.AllAttribute"))
                                        .TakeWhile(m => !Equals(m.Message, "------------"))
                                        .ToList();
                errors.AddRange(preMessages.AssertContains(1, @"Simple string"));
                errors.AddRange(preMessages.AssertContains(1, @"AnyCPU"));
                errors.AddRange(preMessages.AssertContains(1, @"UsageLibrary"));
                errors.AddRange(preMessages.AssertContains(2, @"packages\Newtonsoft.Json.10.0.3"));
                errors.AddRange(preMessages.AssertContains(3, @"Newtonsoft.Json"));
                errors.AddRange(preMessages.AssertContains(1, @"10.0.3.0"));
                errors.AddRange(preMessages.AssertContains(4, @"10.0"));

                var postMessages = runner.GetEvents(postBuildTask)
                                         .Messages
                                         .SkipWhile(m => !Equals(m.Message, "Adaptation: DefinitionLibrary.Assembly.AllAttribute"))
                                         .TakeWhile(m => !Equals(m.Message, "------------"))
                                         .ToList();
                errors.AddRange(postMessages.AssertContains(1, @"Simple string"));
                errors.AddRange(postMessages.AssertContains(1, @"AnyCPU"));
                errors.AddRange(postMessages.AssertContains(1, @"UsageLibrary"));
                errors.AddRange(postMessages.AssertContains(2, @"packages\Newtonsoft.Json.10.0.3"));
                errors.AddRange(postMessages.AssertContains(3, @"Newtonsoft.Json"));
                errors.AddRange(postMessages.AssertContains(1, @"10.0.3.0"));
                errors.AddRange(postMessages.AssertContains(4, @"10.0"));

                var preWarnings = runner.GetEvents(preBuildTask).Warnings;
                errors.AddRange(preWarnings.AssertContains(1, "{boo.Platform:2df}"));
                Assert.AreEqual(1, preWarnings.Count);

                var postWarnings = runner.GetEvents(postBuildTask).Warnings;
                errors.AddRange(postWarnings.AssertContains(1, "{boo.Platform:2df}"));
                Assert.AreEqual(1, postWarnings.Count);

                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());
                errors.AddRange(runner.GetEvents(postBuildTask).Custom.AssertEmpty());
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }
    }
}