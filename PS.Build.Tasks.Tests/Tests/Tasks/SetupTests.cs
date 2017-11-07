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
    public class SetupTests
    {
        [Test]
        public void Test()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\SetupProject\");

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

                var preBuildMessages = runner.GetEvents(preBuildTask).Messages;
                errors.AddRange(preBuildMessages.AssertContains(1, "@Setup Base"));
                errors.AddRange(preBuildMessages.AssertContains(1, "@Setup EmptyAttribute"));
                errors.AddRange(preBuildMessages.AssertContains(1, "@Setup PostBuildAttribute"));
                errors.AddRange(preBuildMessages.AssertContains(1, "@Setup PreBuildAttribute"));
                errors.AddRange(preBuildMessages.AssertContains(1, "@PreBuildAttribute"));

                if (!errors.Any())
                {
                    for (int i = 0; i < preBuildMessages.Count; i++)
                    {
                        if(preBuildMessages[i].Message.Contains("@Setup base")) break;

                        if (preBuildMessages[i].Message.Contains("@Setup EmptyAttribute") ||
                            preBuildMessages[i].Message.Contains("@Setup PostBuildAttribute") ||
                            preBuildMessages[i].Message.Contains("@Setup PreBuildAttribute"))
                        {
                            errors.Add($"Wrong setup call order, {preBuildMessages[i].Message} was before base");
                        }
                    }
                }


                var postBuildMessages = runner.GetEvents(postBuildTask).Messages;
                errors.AddRange(postBuildMessages.AssertContains(1, "@PostBuildAttribute"));

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