using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PS.Build.Services;
using PS.Build.Tasks.Extensions;
using PS.Build.Tasks.Services;
using PS.Build.Types;

namespace PS.Build.Tasks
{
    public class SandboxClient : MarshalByRefObject
    {
        private readonly IExplorer _explorer;
        private CSharpCompilation _compilation;
        private List<AdaptationUsage> _usages;

        #region Constructors

        public SandboxClient(IExplorer explorer)
        {
            if (explorer == null) throw new ArgumentNullException(nameof(explorer));

            _explorer = explorer;

            var assemblyReferences = _explorer.References.Select(i => i.FullPath).ToArray();
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var queryAssemblyName = args.Name.Split(',').FirstOrDefault();
                var resolved = assemblyReferences.FirstOrDefault(r => string.Equals(Path.GetFileNameWithoutExtension(r),
                                                                                    queryAssemblyName,
                                                                                    StringComparison.InvariantCultureIgnoreCase));
                var location = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(resolved))
                {
                    if (string.IsNullOrEmpty(location)) return null;
                    location = Path.GetDirectoryName(location);
                    if (string.IsNullOrEmpty(location)) return null;
                    resolved = Directory.GetFiles(location, "*.dll")
                                        .FirstOrDefault(r => string.Equals(Path.GetFileNameWithoutExtension(r),
                                                                           queryAssemblyName,
                                                                           StringComparison.InvariantCultureIgnoreCase));
                }
                if (string.IsNullOrWhiteSpace(resolved)) return null;

                return Assembly.LoadFile(resolved);
            };
        }

        #endregion

        #region Members

        public void ExecutePostBuildAdaptations(ILogger logger)
        {
            ExecutePostBuildAdaptations(logger, _usages);
        }

        //public SerializableArtifact[] ExecutePreBuildAdaptations(ILogger logger)
        //{
        //    var artifacts = new List<Artifact>();
        //    artifacts.AddRange(ExecutePreBuildAdaptations(_usages, logger));

        //    using (var cacheManager = new CacheManager<ArtifactCache>(_explorer.Directories[BuildDirectory.Intermediate], logger))
        //    {
        //        HandleArtifactsContent(artifacts, cacheManager, logger);
        //        artifacts.Add(new Artifact(cacheManager.GetCachePath(), BuildItem.Internal));
        //    }

        //    return artifacts.Select(a => a.Serialize()).ToArray();
        //}

        public SerializableArtifact[] ExecutePreBuildAdaptations(ILogger logger)
        {
            var result = new List<SerializableArtifact>();

            var adaptationTypes = SearchCompiledAdaptations(logger);
            if (!adaptationTypes.Any())
            {
                logger.Info("Assembly references do not contains adaptation attributes.");
                return result.ToArray();
            }

            var syntaxTrees = CreateSyntaxTrees();

            var suspiciousAttributeSyntaxes = AnalyzeSyntaxForAdaptationUsages(adaptationTypes, syntaxTrees, logger);
            if (!suspiciousAttributeSyntaxes.Any())
            {
                logger.Info("Assembly does not use any defined adaptation attribute.");
                return result.ToArray();
            }

            _compilation = CreateCompilation(syntaxTrees, logger);
            if (_compilation == null) throw new Exception("Can not create compilation");

            _usages = AnalyzeSemanticForAdaptationUsages(suspiciousAttributeSyntaxes, _compilation, logger);
            if (!_usages.Any())
            {
                logger.Info("Assembly does not use any defined adaptation attribute.");
                return result.ToArray();
            }

            var artifacts = new List<Artifact>();
            artifacts.AddRange(ExecutePreBuildAdaptations(_usages, logger));

            using (var cacheManager = new CacheManager<ArtifactCache>(_explorer.Directories[BuildDirectory.Intermediate], logger))
            {
                HandleArtifactsContent(artifacts, cacheManager, logger);
                artifacts.Add(new Artifact(cacheManager.GetCachePath(), BuildItem.Internal));
            }

            result.AddRange(artifacts.Select(a => a.Serialize()));

            return result.ToArray();
        }

        private List<AdaptationUsage> AnalyzeSemanticForAdaptationUsages(SuspiciousAttributeSyntaxes suspiciousAttributeSyntaxes,
                                                                         CSharpCompilation compilation,
                                                                         ILogger logger)
        {
            logger.Debug("------------");
            logger.Info("Analyzing semantic");

            var suspiciousSyntaxTrees = suspiciousAttributeSyntaxes.ToLookup(pair => pair.Key.SyntaxTree, pair => pair);
            var usages = new List<AdaptationUsage>();
            foreach (var group in suspiciousSyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(group.Key, true);

                foreach (var pair in group)
                {
                    var attributeInfo = semanticModel.GetSymbolInfo(pair.Key);
                    var resolvedType = pair.Value.FirstOrDefault(t => attributeInfo.Symbol.IsEquivalent(t));
                    var attributeData = pair.Key.ResolveAttributeData(semanticModel);

                    if (resolvedType == null)
                    {
                        logger.Warn($"Could not resolve '{pair.Key}' type");
                        continue;
                    }

                    usages.Add(new AdaptationUsage(semanticModel,
                                                   group.Key,
                                                   pair.Key.Parent.Parent,
                                                   attributeData,
                                                   resolvedType));
                }
            }
            logger.Info($"Found {usages.Count} adaptation attribute usages");
            foreach (var usage in usages)
            {
                logger.Debug($"+ Usage: {usage}");
            }

            return usages;
        }

        private SuspiciousAttributeSyntaxes AnalyzeSyntaxForAdaptationUsages(IEnumerable<Type> adaptationTypes,
                                                                             IEnumerable<SyntaxTree> syntaxTrees,
                                                                             ILogger logger)
        {
            logger.Debug("------------");
            logger.Info("Analyzing syntax");

            var visitor = new AttributeVirtualizationVisitor(adaptationTypes);
            foreach (var syntaxTree in syntaxTrees)
            {
                visitor.Visit(syntaxTree.GetRoot());
            }

            var suspiciousAttributeSyntaxes = visitor.SuspiciousAttributeSyntaxes;
            logger.Info($"Found {suspiciousAttributeSyntaxes.Count} suspicious attributes");
            foreach (var pair in suspiciousAttributeSyntaxes)
            {
                logger.Debug($" + Syntax: {pair.Key}");
                foreach (var type in pair.Value)
                {
                    logger.Debug($"   * Could be: {type.FullName}");
                }
            }

            return suspiciousAttributeSyntaxes;
        }

        private CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> syntaxTrees, ILogger logger)
        {
            logger.Debug("------------");
            logger.Info("Creating Roslyn compilation");

            var references = _explorer.References.Select(r => MetadataReference.CreateFromFile(r.FullPath));
            return CSharpCompilation.Create(Guid.NewGuid().ToString("N"), syntaxTrees, references);
        }

        private List<SyntaxTree> CreateSyntaxTrees()
        {
            var compiles = _explorer.Items[BuildItem.Compile].Select(r => File.ReadAllText(r.FullPath)).ToArray();
            var syntaxTrees = compiles.Select(c => CSharpSyntaxTree.ParseText(c)).ToList();
            return syntaxTrees;
        }

        private void ExecutePostBuildAdaptations(ILogger logger, List<AdaptationUsage> usages)
        {
            if (usages.All(u => u.PreBuildMethod == null))
            {
                logger.Info("There is no discovered adaptations with post build instructions");
                return;
            }

            logger.Info("Executing discovered adaptations with post build instructions");

            var nugetExplorer = new NugetExplorer(_explorer.Directories[BuildDirectory.Solution]);

            foreach (var usage in usages)
            {
                logger.Debug($"Adaptation: {usage.AttributeData}");
                var method = usage.PostBuildMethod;
                if (method == null)
                {
                    logger.Debug("Adaptation does not contain void PostBuild(IServiceProvider provider) method. Skipping...");
                    continue;
                }

                var attribute = usage.Attribute;
                if (attribute == null)
                {
                    try
                    {
                        attribute = usage.AttributeData.CreateAttribute(usage.Type);
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Could not create adaptation attribute. Details: {e.GetBaseException().Message}.");
                        continue;
                    }
                }

                try
                {
                    var serviceProvider = new ServiceProvider();
                    serviceProvider.AddService(typeof(CSharpCompilation), _compilation);
                    serviceProvider.AddService(typeof(SyntaxNode), usage.AssociatedSyntaxNode);
                    serviceProvider.AddService(typeof(SyntaxTree), usage.SyntaxTree);
                    serviceProvider.AddService(typeof(SemanticModel), usage.SemanticModel);
                    serviceProvider.AddService(typeof(ILogger), logger);
                    serviceProvider.AddService(typeof(IExplorer), _explorer);
                    serviceProvider.AddService(typeof(INugetExplorer), nugetExplorer);

                    method.Invoke(attribute, new object[] { serviceProvider });
                }
                catch (Exception e)
                {
                    logger.Error($"Adaptation failed. Details: {e.GetBaseException().Message}.");
                }
            }
        }

        private List<Artifact> ExecutePreBuildAdaptations(List<AdaptationUsage> usages,
                                                          ILogger logger)
        {
            var result = new List<Artifact>();

            if (usages.All(u => u.PreBuildMethod == null))
            {
                logger.Info("There is no discovered adaptations with pre build instructions");
                return result;
            }

            logger.Info("Executing discovered adaptations with pre build instructions");
            var nugetExplorer = new NugetExplorer(_explorer.Directories[BuildDirectory.Solution]);

            foreach (var usage in usages)
            {
                logger.Debug($"Adaptation: {usage.AttributeData}");
                var method = usage.PreBuildMethod;
                if (method == null)
                {
                    logger.Debug("Adaptation does not contain void PreBuild(IServiceProvider provider) method. Skipping...");
                    continue;
                }

                Attribute attribute;
                try
                {
                    attribute = usage.AttributeData.CreateAttribute(usage.Type);
                    usage.Attribute = attribute;
                }
                catch (Exception e)
                {
                    logger.Warn($"Could not create adaptation attribute. Details: {e.GetBaseException().Message}.");
                    continue;
                }

                try
                {
                    var artifactory = new Artifactory();

                    var serviceProvider = new ServiceProvider();
                    serviceProvider.AddService(typeof(CSharpCompilation), _compilation);
                    serviceProvider.AddService(typeof(SyntaxNode), usage.AssociatedSyntaxNode);
                    serviceProvider.AddService(typeof(SyntaxTree), usage.SyntaxTree);
                    serviceProvider.AddService(typeof(SemanticModel), usage.SemanticModel);
                    serviceProvider.AddService(typeof(ILogger), logger);
                    serviceProvider.AddService(typeof(IExplorer), _explorer);
                    serviceProvider.AddService(typeof(IArtifactory), artifactory);
                    serviceProvider.AddService(typeof(INugetExplorer), nugetExplorer);

                    method.Invoke(attribute, new object[] { serviceProvider });

                    result.AddRange(artifactory.Artifacts);
                }
                catch (Exception e)
                {
                    logger.Error($"Adaptation failed. Details: {e.GetBaseException().Message}.");
                }
            }
            return result;
        }

        private void HandleArtifactsContent(List<Artifact> artifacts, CacheManager<ArtifactCache> cacheManager, ILogger logger)
        {
            var artifactsWithContentGenerators = artifacts.Where(a => a.ContentFactory != null).ToList();
            if (!artifactsWithContentGenerators.Any()) return;

            logger.Debug("------------");
            logger.Info("Processing adaptation artifacts content");

            foreach (var artifact in artifactsWithContentGenerators)
            {
                try
                {
                    var currentHashCodes = artifact.Dependencies.GetDependenciesHashCodes().ToArray();
                    var cache = cacheManager.GetCached(artifact.Path);
                    if (cache == null || !File.Exists(artifact.Path) || !currentHashCodes.Any())
                    {
                        cacheManager.Cache(artifact.Path,
                                           new ArtifactCache
                                           {
                                               HashCodes = currentHashCodes
                                           });
                    }
                    else if (cache.HashCodes.SequenceEqual(currentHashCodes))
                    {
                        logger.Info($"+ {artifact}: Content is up to date");
                        continue;
                    }

                    var content = artifact.ContentFactory();
                    if (content != null)
                    {
                        Path.GetDirectoryName(artifact.Path).EnsureDirectoryExist();
                        File.WriteAllBytes(artifact.Path, content);
                    }
                    logger.Info($"+ {artifact}: Content generated");
                }
                catch (Exception e)
                {
                    logger.Error($"Adaptation content generation failed. Details: {e.GetBaseException().Message}.");
                }
            }
        }

        private List<Type> SearchCompiledAdaptations(ILogger logger)
        {
            var references = _explorer.References.Select(i => i.FullPath).ToList();
            var adaptationDefinitionTypes = new List<Type>();

            logger.Debug("------------");
            logger.Info("Searching for compiled adaptation attributes");

            var pathBanns = new List<string>
            {
                @"Reference Assemblies\Microsoft\Framework\.NETFramework"
            };

            foreach (var reference in references)
            {
                if (pathBanns.Any(p => reference.Contains(p))) continue;

                try
                {
                    var assembly = Assembly.LoadFile(reference);
                    var foundTypes = assembly.GetTypes()
                                             .Where(t => typeof(Attribute).IsAssignableFrom(t))
                                             .Where(t => t.GetCustomAttribute<DesignerAttribute>()?.DesignerTypeName == "PS.Build.Adaptation")
                                             .Where(t => t.GetCustomAttribute<AttributeUsageAttribute>()?.Inherited == false);

                    adaptationDefinitionTypes.AddRange(foundTypes);
                }
                catch (Exception e)
                {
                    logger.Warn($"Could not load reference assembly '{reference}'. Details: {e.GetBaseException()}");
                }
            }

            var adaptationTypes = AppDomain.CurrentDomain
                                           .GetAssemblies()
                                           .Where(a => pathBanns.Any(p => a.Location?.Contains(p) != true))
                                           .SelectMany(a => a.GetTypes().Where(t => adaptationDefinitionTypes.Any(adf => adf.IsAssignableFrom(t))))
                                           .Where(t => !t.IsAbstract)
                                           .ToList();

            logger.Info($"Found {adaptationTypes.Count} adaptation attribute definitions");
            foreach (var t in adaptationTypes)
            {
                logger.Debug($"+ Definition: {t.FullName} in {t.Assembly.GetName().Name}");
            }

            return adaptationTypes;
        }

        #endregion
    }
}