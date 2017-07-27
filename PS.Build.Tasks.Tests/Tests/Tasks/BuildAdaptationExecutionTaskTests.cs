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
        private static Dictionary<AttributeTargets, List<Tuple<string, int>>> ExpectedOrderedMap
        {
            get
            {
                var expectedMap = new Dictionary<string, List<Tuple<AttributeTargets, int>>>
                {
                    {
                        "Assembly.cs", new List<Tuple<AttributeTargets, int>>
                        {
                            new Tuple<AttributeTargets, int>(AttributeTargets.Assembly, 1)
                        }
                    },
                    {
                        "Class.cs", new List<Tuple<AttributeTargets, int>>
                        {
                            new Tuple<AttributeTargets, int>(AttributeTargets.Class, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.GenericParameter, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Field, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Constructor, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Parameter, 3),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Property, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.ReturnValue, 3),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Method, 3),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Event, 1),
                        }
                    },
                    {
                        "Enum.cs", new List<Tuple<AttributeTargets, int>>
                        {
                            new Tuple<AttributeTargets, int>(AttributeTargets.Enum, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Field, 1)
                        }
                    },
                    {
                        "IInterface.cs", new List<Tuple<AttributeTargets, int>>
                        {
                            new Tuple<AttributeTargets, int>(AttributeTargets.Interface, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.GenericParameter, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Property, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Parameter, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.ReturnValue, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Method, 5),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Event, 2),
                        }
                    },
                    {
                        "Module.cs", new List<Tuple<AttributeTargets, int>>
                        {
                            new Tuple<AttributeTargets, int>(AttributeTargets.Module, 1)
                        }
                    },
                    {
                        "Struct.cs", new List<Tuple<AttributeTargets, int>>
                        {
                            new Tuple<AttributeTargets, int>(AttributeTargets.Struct, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.GenericParameter, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Field, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Constructor, 1),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Parameter, 3),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Property, 2),
                            new Tuple<AttributeTargets, int>(AttributeTargets.ReturnValue, 3),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Method, 5),
                            new Tuple<AttributeTargets, int>(AttributeTargets.Event, 2),
                        }
                    },
                };
                var reorderedMap = new Dictionary<AttributeTargets, List<Tuple<string, int>>>();
                foreach (AttributeTargets value in Enum.GetValues(typeof(AttributeTargets)))
                {
                    if (value == AttributeTargets.All) continue;
                    var group = expectedMap.Select(pair =>
                    {
                        var existing = pair.Value.FirstOrDefault(v => v.Item1 == value);
                        if (existing == null) return null;
                        return new Tuple<string, int>(pair.Key, existing.Item2);
                    }).Where(i => i != null).ToList();

                    reorderedMap.Add(value, group);
                }

                return reorderedMap;
            }
        }

        private static IEnumerable<string> GetSequenceMessages(AttributeTargets targets, List<Tuple<string, int>> items)
        {
            foreach (var item in items)
            {
                for (int i = 0; i < item.Item2; i++)
                {
                    yield return string.Join(",", targets, "AllAttribute", item.Item1);
                }
            }
        }

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
                                                               $"DefinitionLibrary.{value}.EmptyAttribute has no PreBuid or PostBuild entries. Skipping..."));

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

                var sequence = new List<object>();

                //Assembly attributes first
                sequence.AddRange(GetSequenceMessages(AttributeTargets.Assembly, ExpectedOrderedMap[AttributeTargets.Assembly]));

                //Module attributes last
                sequence.AddRange(GetSequenceMessages(AttributeTargets.Module, ExpectedOrderedMap[AttributeTargets.Module]));
            }
            if (errors.Any()) Assert.Fail(string.Join(Environment.NewLine, errors));
        }
    }
}