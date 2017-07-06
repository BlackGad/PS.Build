using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PS.Build.Services;
using PS.Build.Tasks.Common;
using PS.Build.Tasks.Extensions;
using PS.Build.Tasks.Services;
using PS.Build.Types;

namespace PS.Build.Tasks
{
    public class SandboxClient : MarshalByRefObject
    {
        #region Static members

        private static MethodInfo GetAdaptMethod(Type t)
        {
            return t.GetMethod("Adapt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        #endregion

        private readonly IExplorer _explorer;
        private readonly ILogger _logger;

        #region Constructors

        public SandboxClient(ILogger logger, IExplorer explorer)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (explorer == null) throw new ArgumentNullException(nameof(explorer));

            _logger = logger;
            _explorer = explorer;

            var assemblyReferences = _explorer.References.Select(i => i.FullPath).ToArray();
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var queryAssemblyName = args.Name.Split(',').FirstOrDefault();
                var resolvedAssemblyName = assemblyReferences.FirstOrDefault(r => string.Equals(Path.GetFileNameWithoutExtension(r),
                                                                                                queryAssemblyName,
                                                                                                StringComparison.InvariantCultureIgnoreCase));
                return !string.IsNullOrWhiteSpace(resolvedAssemblyName)
                    ? Assembly.LoadFile(resolvedAssemblyName)
                    : null;
            };
        }

        #endregion

        #region Members

        public SerializableArtifact[] Execute()
        {
            var adaptationTypes = SearchCompiledAdaptations();
            if (!adaptationTypes.Any())
            {
                _logger.Info("Assembly references do not contains adaptation attributes.");
                return Enumerable.Empty<SerializableArtifact>().ToArray();
            }

            var syntaxTrees = CreateSyntaxTrees();

            var suspiciousAttributeSyntaxes = AnalyzeSyntaxForAdaptationUsages(adaptationTypes, syntaxTrees);
            if (!suspiciousAttributeSyntaxes.Any())
            {
                _logger.Info("Assembly does not use any defined adaptation attribute.");
                return Enumerable.Empty<SerializableArtifact>().ToArray();
            }

            var compilation = CreateCompilation(syntaxTrees);
            if (compilation == null) throw new Exception("Can not create semantic model");

            var usages = AnalyzeSemanticForAdaptationUsages(suspiciousAttributeSyntaxes, compilation);
            if (!usages.Any())
            {
                _logger.Info("Assembly does not use any defined adaptation attribute.");
                return Enumerable.Empty<SerializableArtifact>().ToArray();
            }

            var artifacts = new List<Artifact>();
            artifacts.AddRange(ExecuteAdaptations(usages, compilation));

            using (var cacheManager = new CacheManager<ArtifactCache>(_explorer.Directories[BuildDirectory.Intermediate], _logger))
            {
                HandleArtifactsContent(artifacts, cacheManager);
                artifacts.Add(new Artifact(cacheManager.GetCachePath(), BuildItem.Internal));
            }

            return artifacts.Select(a => a.Serialize()).ToArray();
        }

        private List<AdaptationUsage> AnalyzeSemanticForAdaptationUsages(SuspiciousAttributeSyntaxes suspiciousAttributeSyntaxes,
                                                                         CSharpCompilation compilation)
        {
            _logger.Debug("------------");
            _logger.Info("Analyzing semantic");

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

                    usages.Add(new AdaptationUsage(semanticModel,
                                                   group.Key,
                                                   pair.Key.Parent.Parent,
                                                   attributeData,
                                                   resolvedType));
                }
            }
            _logger.Info($"Found {usages.Count} adaptation attribute usages");
            foreach (var usage in usages)
            {
                _logger.Debug($"+ Usage: {usage}");
            }

            return usages;
        }

        private SuspiciousAttributeSyntaxes AnalyzeSyntaxForAdaptationUsages(IEnumerable<Type> adaptationTypes,
                                                                             IEnumerable<SyntaxTree> syntaxTrees)
        {
            _logger.Debug("------------");
            _logger.Info("Analyzing syntax");

            var visitor = new AttributeVirtualizationVisitor(adaptationTypes);
            foreach (var syntaxTree in syntaxTrees)
            {
                visitor.Visit(syntaxTree.GetRoot());
            }

            var suspiciousAttributeSyntaxes = visitor.SuspiciousAttributeSyntaxes;
            _logger.Info($"Found {suspiciousAttributeSyntaxes.Count} suspicious attributes");
            foreach (var pair in suspiciousAttributeSyntaxes)
            {
                _logger.Debug($" + Syntax: {pair.Key}");
                foreach (var type in pair.Value)
                {
                    _logger.Debug($"   * Could be: {type.FullName}");
                }
            }

            return suspiciousAttributeSyntaxes;
        }

        private CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> syntaxTrees)
        {
            _logger.Debug("------------");
            _logger.Info("Creating Roslyn compilation");

            var references = _explorer.References.Select(r => MetadataReference.CreateFromFile(r.FullPath));
            return CSharpCompilation.Create(Guid.NewGuid().ToString("N"), syntaxTrees, references);
        }

        private List<SyntaxTree> CreateSyntaxTrees()
        {
            var compiles = _explorer.Items[BuildItem.Compile].Select(r => File.ReadAllText(r.FullPath)).ToArray();
            var syntaxTrees = compiles.Select(c => CSharpSyntaxTree.ParseText(c)).ToList();
            return syntaxTrees;
        }

        private List<Artifact> ExecuteAdaptations(List<AdaptationUsage> usages, CSharpCompilation compilation)
        {
            _logger.Debug("------------");
            _logger.Info("Executing discovered adaptations");

            var nugetExplorer = new NugetExplorer(_explorer.Directories[BuildDirectory.Solution]);

            var result = new List<Artifact>();
            foreach (var usage in usages)
            {
                _logger.Debug($"Adaptation: {usage.AttributeData}");
                Attribute attribute;
                try
                {
                    attribute = usage.AttributeData.CreateAttribute(usage.Type);
                }
                catch (Exception e)
                {
                    _logger.Warn($"Could not create adaptation attribute. Details: {e.GetBaseException().Message}.");
                    continue;
                }

                try
                {
                    var method = GetAdaptMethod(usage.Type);
                    if (method == null) throw new InvalidOperationException();

                    var artifactory = new Artifactory();

                    var serviceProvider = new ServiceProvider();
                    serviceProvider.AddService(typeof(CSharpCompilation), compilation);
                    serviceProvider.AddService(typeof(SyntaxNode), usage.AssociatedSyntaxNode);
                    serviceProvider.AddService(typeof(SyntaxTree), usage.SyntaxTree);
                    serviceProvider.AddService(typeof(SemanticModel), usage.SemanticModel);
                    serviceProvider.AddService(typeof(ILogger), _logger);
                    serviceProvider.AddService(typeof(IExplorer), _explorer);
                    serviceProvider.AddService(typeof(IArtifactory), artifactory);
                    serviceProvider.AddService(typeof(INugetExplorer), nugetExplorer);

                    method.Invoke(attribute, new object[] { serviceProvider });

                    result.AddRange(artifactory.Artifacts);
                }
                catch (Exception e)
                {
                    _logger.Warn($"Adaptation failed. Details: {e.GetBaseException().Message}.");
                }
            }
            return result;
        }

        private void HandleArtifactsContent(List<Artifact> artifacts, CacheManager<ArtifactCache> cacheManager)
        {
            var artifactsWithContentGenerators = artifacts.Where(a => a.ContentFactory != null).ToList();
            if (!artifactsWithContentGenerators.Any()) return;

            _logger.Debug("------------");
            _logger.Info("Processing adaptation artifacts content");

            foreach (var artifact in artifactsWithContentGenerators)
            {
                try
                {
                    var currentHashCodes = artifact.Dependencies.GetDependenciesHashCodes().ToArray();
                    var cache = cacheManager.GetCached(artifact.Path);
                    if (cache == null || !File.Exists(artifact.Path))
                    {
                        cacheManager.Cache(artifact.Path,
                                           new ArtifactCache
                                           {
                                               HashCodes = currentHashCodes
                                           });
                    }
                    else if (cache.HashCodes.SequenceEqual(currentHashCodes))
                    {
                        _logger.Info($"+ {artifact}: Content is up to date");
                        continue;
                    }

                    var content = artifact.ContentFactory();
                    if (content != null)
                    {
                        Path.GetDirectoryName(artifact.Path).EnsureDirectoryExist();
                        File.WriteAllBytes(artifact.Path, content);
                    }
                    _logger.Info($"+ {artifact}: Content generated");
                }
                catch (Exception e)
                {
                    _logger.Warn($"Adaptation content generation failed. Details: {e.GetBaseException().Message}.");
                }
            }
        }

        private List<Type> SearchCompiledAdaptations()
        {
            var references = _explorer.References.Select(i => i.FullPath).ToList();
            var adaptationDefinitionTypes = new List<Type>();

            _logger.Debug("------------");
            _logger.Info("Searching for compiled adaptation attributes");

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
                                             .Where(t =>
                                             {
                                                 var usageAttribute = t.GetCustomAttribute<AttributeUsageAttribute>() ??
                                                                      new AttributeUsageAttribute(AttributeTargets.All);

                                                 if (usageAttribute.Inherited) return false;

                                                 var adaptMethod = GetAdaptMethod(t);
                                                 var parameters = adaptMethod?.GetParameters();
                                                 if (parameters?.Length != 1) return false;
                                                 return parameters.First().ParameterType == typeof(IServiceProvider);
                                             });

                    adaptationDefinitionTypes.AddRange(foundTypes);
                }
                catch (Exception e)
                {
                    _logger.Warn($"Could not load reference assembly '{reference}'. Details: {e.GetBaseException()}");
                }
            }

            var adaptationTypes = AppDomain.CurrentDomain
                                           .GetAssemblies()
                                           .Where(a => pathBanns.Any(p => a.Location?.Contains(p) != true))
                                           .SelectMany(a => a.GetTypes().Where(t => adaptationDefinitionTypes.Any(adf => adf.IsAssignableFrom(t))))
                                           .Where(t => !t.IsAbstract)
                                           .ToList();

            _logger.Info($"Found {adaptationTypes.Count} adaptation attribute definitions");
            foreach (var t in adaptationTypes)
            {
                _logger.Debug($"+ Definition: {t.FullName} in {t.Assembly.GetName().Name}");
            }

            return adaptationTypes;
        }

        #endregion
    }
}