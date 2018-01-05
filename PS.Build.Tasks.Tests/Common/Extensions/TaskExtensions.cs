using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using PS.Build.Extensions;

namespace PS.Build.Tasks.Tests.Common.Extensions
{
    public static class TaskExtensions
    {
        #region Static members

        public static void Setup(this PreBuildAdaptationExecutionTask preBuildTask,
                                 TestProject project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            var usageProjectDirectory = Path.GetDirectoryName(project.Path);
            Assert.IsNotNull(usageProjectDirectory, $"{nameof(usageProjectDirectory)} is invalid");

            var xProject = XDocument.Load(project.Path);
            Assert.IsNotNull(xProject, $"{nameof(xProject)} was not loaded from {project.Path}");

            var msbuildNS = "http://schemas.microsoft.com/developer/msbuild/2003";
            var outputPath = xProject.Descendants(XName.Get("OutputPath", msbuildNS))
                                     .FirstOrDefault(e => e.Parent?.Attributes().Any(a => a.Value.Contains(project.Solution.Configuration)) == true)?
                                     .Value;

            Assert.IsNotNull(outputPath, $"{nameof(outputPath)} was not found in {project.Path} project file");

            preBuildTask.PropertyProjectFile = project.Path;
            preBuildTask.PropertyPlatform = project.Solution.Platform;
            preBuildTask.PropertyConfiguration = project.Solution.Configuration;
            preBuildTask.PropertyRootNamespace = xProject.Descendants(XName.Get("RootNamespace", msbuildNS)).FirstOrDefault()?.Value;
            preBuildTask.PropertyDefineConstants = xProject.Descendants(XName.Get("DefineConstants", msbuildNS)).FirstOrDefault()?.Value;

            preBuildTask.DirectoryIntermediate = "obj\\" + project.Solution.Configuration;
            preBuildTask.DirectoryProject = usageProjectDirectory;
            preBuildTask.DirectoryTarget = Path.Combine(usageProjectDirectory, outputPath);
            preBuildTask.DirectorySolution = project.Solution.SolutionDirectory;

            foreach (var pair in PreBuildAdaptationExecutionTask.ItemsProperties)
            {
                if (pair.Value == null) continue;

                var items = xProject.Descendants(XName.Get(pair.Key.ToString(), msbuildNS))
                                    .Select(n =>
                                    {
                                        var relativePath = n.Attribute(XName.Get("Include"))?.Value ?? string.Empty;
                                        return new TaskItem(Path.Combine(usageProjectDirectory, relativePath));
                                    })
                                    .Enumerate<ITaskItem>()
                                    .ToArray();

                pair.Value.SetValue(preBuildTask, items);
            }
        }

        #endregion
    }
}