using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PS.Build.Tasks.Extensions;
using PS.Build.Tasks.Services;
using PS.Build.Types;
using Logger = PS.Build.Tasks.Services.Logger;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace PS.Build.Tasks
{
    public class PreBuildAdaptationExecutionTask : Task
    {
        #region Constants

        public static readonly Dictionary<BuildItem, PropertyInfo> ArtifactsProperties;
        public static readonly Dictionary<BuildDirectory, PropertyInfo> DirectoryProperties;
        public static readonly Dictionary<BuildItem, PropertyInfo> ItemsProperties;
        public static readonly Dictionary<BuildProperty, PropertyInfo> PropertiesProperties;

        #endregion

        #region Static members

        private static string FindSolutionDirectory(string projectFile, string projectDirectory)
        {
            var directory = projectDirectory;
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    foreach (var file in Directory.EnumerateFiles(Path.Combine(directory), "*.sln"))
                    {
                        if (File.ReadAllText(file).Contains(projectFile)) return Path.GetDirectoryName(file);
                    }
                    directory = Path.Combine(directory, "..\\");
                }
            }
            catch (Exception)
            {
                //Nothing
            }

            return projectDirectory;
        }

        #endregion

        #region Constructors

        static PreBuildAdaptationExecutionTask()
        {
            var type = typeof(PreBuildAdaptationExecutionTask);
            ItemsProperties = Enum.GetValues(typeof(BuildItem))
                                  .OfType<BuildItem>()
                                  .ToDictionary(v => v, v => type.GetProperty($"Items{v}"));

            DirectoryProperties = Enum.GetValues(typeof(BuildDirectory))
                                      .OfType<BuildDirectory>()
                                      .ToDictionary(v => v, v => type.GetProperty($"Directory{v}"));

            PropertiesProperties = Enum.GetValues(typeof(BuildProperty))
                                       .OfType<BuildProperty>()
                                       .ToDictionary(v => v, v => type.GetProperty($"Property{v}"));
            ArtifactsProperties = Enum.GetValues(typeof(BuildItem))
                                      .OfType<BuildItem>()
                                      .ToDictionary(v => v, v => type.GetProperty($"Artifacts{v}"));
        }

        #endregion

        #region Properties

        [Output]
        public ITaskItem[] ArtifactsAdditionalFiles { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsCompile { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsContent { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsEmbeddedResource { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsEntityDeploy { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsNone { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsPage { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsResource { get; private set; }

        [Output]
        public ITaskItem[] ArtifactsTemporary { get; private set; }

        [Required]
        public string DirectoryIntermediate { get; set; }

        [Required]
        public string DirectoryProject { get; set; }

        [Required]
        public string DirectorySolution { get; set; }

        [Required]
        public string DirectoryTarget { get; set; }

        [Required]
        public ITaskItem[] ItemsAdditionalFiles { get; set; }

        [Required]
        public ITaskItem[] ItemsCompile { get; set; }

        [Required]
        public ITaskItem[] ItemsContent { get; set; }

        [Required]
        public ITaskItem[] ItemsEmbeddedResource { get; set; }

        [Required]
        public ITaskItem[] ItemsEntityDeploy { get; set; }

        [Required]
        public ITaskItem[] ItemsNone { get; set; }

        [Required]
        public ITaskItem[] ItemsPage { get; set; }

        [Required]
        public ITaskItem[] ItemsResource { get; set; }

        public bool OptionDebug { get; set; }

        [Required]
        public string PropertyConfiguration { get; set; }

        [Required]
        public string PropertyPlatform { get; set; }

        [Required]
        public string PropertyProjectFile { get; set; }

        [Required]
        public string PropertyRootNamespace { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        #endregion

        #region Override members

        public override bool Execute()
        {
            if (!Debugger.IsAttached && OptionDebug) Debugger.Launch();

            var logger = new Logger(Log);
            try
            {
                var items = ItemsProperties.Enumerate().ToDictionary(pair => pair.Key,
                                                                     pair =>
                                                                     {
                                                                         var taskItems = pair.Value?.GetValue(this) as IEnumerable<ITaskItem>;
                                                                         if (taskItems == null) return Enumerable.Empty<Item>();
                                                                         return taskItems.Select(c => new Item(c));
                                                                     });

                var references = References.Enumerate().Select(c => new Item(c));
                var properties = PropertiesProperties.Enumerate().ToDictionary(pair => pair.Key, pair => pair.Value?.GetValue(this) as string);
                var directories = DirectoryProperties.Enumerate().ToDictionary(pair => pair.Key, pair => pair.Value?.GetValue(this) as string);
                //Check solution folder property
                if (string.IsNullOrWhiteSpace(directories[BuildDirectory.Solution]) || directories[BuildDirectory.Solution] == "*Undefined*")
                {
                    directories[BuildDirectory.Solution] = FindSolutionDirectory(properties[BuildProperty.ProjectFile],
                                                                                 directories[BuildDirectory.Project]);
                }

                //Normilize all pathes and make sure all directories has slash
                foreach (var directory in directories.Keys.ToArray())
                {
                    var sourceDirectory = directories[directory];
                    if (sourceDirectory == null) continue;
                    if (!sourceDirectory.IsAbsolutePath()) sourceDirectory = Path.Combine(directories[BuildDirectory.Project], sourceDirectory);
                    directories[directory] = sourceDirectory.NormalizePath().TrimEnd('\\') + "\\";
                }

                var explorer = new Explorer(references,
                                            items,
                                            directories,
                                            properties);

                var sandbox = new Sanbox(explorer);
                BuildEngine4.RegisterTaskObject(typeof(Sanbox), sandbox, RegisteredTaskObjectLifetime.Build, false);
                var artifacts = sandbox.Client.ExecutePreBuildAdaptations(logger);
                if (artifacts.Any())
                {
                    logger.Debug("------------");
                    logger.Info("Adding additional TaskItems to build");

                    foreach (var builder in artifacts)
                    {
                        logger.Info($"+ {builder}");
                    }

                    FillOutputTaskItems(artifacts);
                }

                ArtifactsTemporary = artifacts.Where(a => !a.IsPermanent)
                                              .Select(a => new TaskItem(a.Path))
                                              .OfType<ITaskItem>()
                                              .ToArray();
            }
            catch (Exception e)
            {
                logger.Error($"Assembly adaptation initialization failed. Details: {e.GetBaseException().Message}");
            }

            return !Log.HasLoggedErrors;
        }

        #endregion

        #region Members

        private void FillOutputTaskItems(SerializableArtifact[] artifacts)
        {
            var groupedArtifacts = artifacts.ToLookup(b => b.Type, b => b);
            foreach (var group in groupedArtifacts)
            {
                if (!ArtifactsProperties.ContainsKey(group.Key)) continue;
                if (ArtifactsProperties[group.Key] == null) continue;
                ArtifactsProperties[group.Key].SetValue(this, group.Select(b => new TaskItem(b.Path, b.Metadata)).ToArray());
            }
        }

        #endregion
    }
}