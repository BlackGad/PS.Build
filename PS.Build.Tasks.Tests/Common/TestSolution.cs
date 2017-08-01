using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PS.Build.Extensions;

namespace PS.Build.Tasks.Tests.Common
{
    public class TestSolution
    {
        #region Constructors

        public TestSolution(string solutionDirectory, string configuration = "Release", string platform = "AnyCPU")
        {
            Projects = new List<TestProject>();
            Assert.IsTrue(Directory.Exists(solutionDirectory), "Solution folder does not exist");
            SolutionDirectory = solutionDirectory;
            Configuration = configuration;
            Platform = platform;
            SolutionPath = Directory.GetFiles(SolutionDirectory, "*.sln").FirstOrDefault();
            Assert.IsNotNull(SolutionPath, $"Solution not found in {SolutionDirectory}");

            var regexOptions = RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace;
            var projectsMatch = Regex.Matches(File.ReadAllText(SolutionPath), "Project.*?EndProject", regexOptions);
            Assert.IsTrue(projectsMatch.Count > 0, "Could not detect projects in solution file");
            foreach (Match match in projectsMatch)
            {
                Assert.IsTrue(match.Success, $"Project entry {match.Value} failed");

                var project = new TestProject(this);

                var projectValues = Regex.Matches(match.Value, "\"([^\"]+)\"", regexOptions)
                                         .Enumerate<Match>()
                                         .Select(m => m.Value.Trim('"'))
                                         .Reverse()
                                         .ToList();

                Assert.GreaterOrEqual(4, projectValues.Count);

                project.ID = Guid.Parse(projectValues.First());
                projectValues = projectValues.Skip(1).ToList();
                project.Path = Path.Combine(SolutionDirectory, projectValues.First());
                projectValues = projectValues.Skip(1).ToList();
                project.Name = projectValues.First();
                projectValues = projectValues.Skip(1).ToList();
                project.Types = projectValues.Select(Guid.Parse).ToArray();

                Assert.IsTrue(File.Exists(project.Path), $"Project file {project.Path} does not exist");
                Projects.Add(project);
            }
        }

        #endregion

        #region Properties

        public string Configuration { get; }
        public string Platform { get; }

        public List<TestProject> Projects { get; }

        public string SolutionDirectory { get; }
        public string SolutionPath { get; }

        #endregion

        #region Members

        public TestProject Project(string name)
        {
            var result = Projects.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(result);
            return result;
        }

        public void RestoreNuget()
        {
            var nugetPath = ProjectBeacon.GetNugetToolPath();

            var arguments = new List<string>()
            {
                "restore",
                $"\"{SolutionPath}\"",
                //$"-SolutionDirectory \"{SolutionDirectory}\"",
                "-NonInteractive"
            };

            var argumentsLine = string.Join(" ", arguments);
            Console.WriteLine(argumentsLine);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(nugetPath, argumentsLine)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
            };

            process.OutputDataReceived += (sender, args) => { if (!string.IsNullOrEmpty(args.Data)) Console.WriteLine(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if (!string.IsNullOrEmpty(args.Data)) Console.WriteLine(args.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        #endregion
    }
}