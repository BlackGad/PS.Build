using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PS.Build.Extensions;
using PS.Build.Tasks.Tests.Common;
using PS.Build.Tasks.Tests.Common.Extensions;

namespace PS.Build.Tasks.Tests.Tasks
{
    [TestFixture]
    public class SourceCodeIsolationTests
    {
        [Test]
        public void Test()
        {
            var targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDirectory = Path.Combine(targetDirectory, @"TestReferences\Projects\SourceCodeIsolationProject\");

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

                errors.AddRange(runner.GetEvents(preBuildTask).Messages.AssertContains(1, string.Join(",", "PreBuild", "Escaped")));
                errors.AddRange(runner.GetEvents(preBuildTask).Messages.AssertContains(2, string.Join(",", "PreBuild", "Mixed")));
                errors.AddRange(runner.GetEvents(preBuildTask).Messages.AssertContains(1, string.Join(",", "PreBuild", "Unescaped")));
                errors.AddRange(runner.GetEvents(preBuildTask).Messages.AssertContains(3, "Suspicious attributes isolation test"));
                errors.AddRange(runner.GetEvents(preBuildTask).Warnings.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(preBuildTask).Custom.AssertEmpty());

                if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));

                var replaceCompileItemsTask = runner.Create<ReplaceCompileItemsTask>();
                replaceCompileItemsTask.Test = true;
                replaceCompileItemsTask.ItemsCompile = preBuildTask.ArtifactsCompile.Enumerate().Union(preBuildTask.ItemsCompile).ToArray();
                replaceCompileItemsTask.Execute();

                errors.AddRange(runner.GetEvents(replaceCompileItemsTask).Messages.AssertContains(1, "2 files will be replaced"));
                errors.AddRange(runner.GetEvents(replaceCompileItemsTask).Warnings.AssertCount(1));
                errors.AddRange(runner.GetEvents(replaceCompileItemsTask).Warnings.AssertContains(1, "Mixed isolation detected in"));
                errors.AddRange(runner.GetEvents(replaceCompileItemsTask).Errors.AssertEmpty());
                errors.AddRange(runner.GetEvents(replaceCompileItemsTask).Custom.AssertEmpty());

                if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));

                Assert.AreEqual(2, replaceCompileItemsTask.CompilesToRemove.Length);
                Assert.AreEqual(2, replaceCompileItemsTask.CompilesToAdd.Length);

                Assert.IsTrue(replaceCompileItemsTask.CompilesToRemove.Count(i => i.ItemSpec.EndsWith("Mixed.cs")) == 1);
                Assert.IsTrue(replaceCompileItemsTask.CompilesToRemove.Count(i => i.ItemSpec.EndsWith("Unescaped.cs")) == 1);
            }
        }
    }
}