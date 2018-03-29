using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PS.Build.Extensions;
using PS.Build.Services;
using PS.Build.Tasks.Extensions;
using PS.Build.Tasks.Services;
using PS.Build.Types;

namespace PS.Build.Tasks
{
    class SandboxClient : MarshalByRefObject
    {
        #region Static members

        private static IEnumerable<AdaptationUsage> Sort(IList<AdaptationUsage> usages)
        {
            var subsets = new List<AdaptationUsage>();
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Assembly));

            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Field));

            var fieldEvents = usages.Where(u =>
            {
                if (u.AttributeTargets != AttributeTargets.Event) return false;
                return u.AssociatedSyntaxNode.Parent.Parent is BaseFieldDeclarationSyntax;
            }).ToList();

            subsets.AddRange(fieldEvents);

            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Parameter));
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.ReturnValue));
            var methodTemplates = usages.Where(u =>
            {
                if (u.AttributeTargets != AttributeTargets.GenericParameter) return false;
                return u.AssociatedSyntaxNode.Parent.Parent is BaseMethodDeclarationSyntax;
            }).ToList();
            subsets.AddRange(methodTemplates);

            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Constructor));
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Method));
            subsets.AddRange(usages.Except(fieldEvents).Where(u => u.AttributeTargets == AttributeTargets.Event));
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Property));

            subsets.AddRange(usages.Except(methodTemplates).Where(u => u.AttributeTargets == AttributeTargets.GenericParameter));

            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Interface));
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Enum));
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Class));
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Struct));
            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Delegate));

            subsets.AddRange(usages.Where(u => u.AttributeTargets == AttributeTargets.Module));
            return subsets;
        }

        #endregion

        private readonly DomainAssemblyResolver _assemblyResolver;

        private readonly IDynamicVault _dynamicVault;
        private readonly IExplorer _explorer;
        private readonly MacroResolver _macroResolver;
        private readonly NugetExplorer _nugetExplorer;
        private CSharpCompilation _compilation;
        private List<AdaptationUsage> _usages;

        #region Constructors

        public SandboxClient(DomainAssemblyResolver assemblyResolver, IExplorer explorer)
        {
            if (assemblyResolver == null) throw new ArgumentNullException(nameof(assemblyResolver));
            if (explorer == null) throw new ArgumentNullException(nameof(explorer));
            _assemblyResolver = assemblyResolver;
            _explorer = explorer;
            _dynamicVault = new DynamicVault();
            _nugetExplorer = new NugetExplorer(_explorer.Directories[BuildDirectory.Solution]);
            _macroResolver = new MacroResolver();
            _macroResolver.Register(new ExplorerMacroHandler(_explorer));
            _macroResolver.Register(new NugetExplorerMacroHandler(_nugetExplorer));
            _macroResolver.Register(new EnvironmentMacroHandler());
            _macroResolver.Register(new TimeMacroHandler());
            _macroResolver.Register(new UidMacroHandler());
            _macroResolver.Register(new SpecialDirectoryMacroHandler());
        }

        #endregion

        #region Members

        public void ExecutePostBuildAdaptations(ILogger logger)
        {
            if (_usages != null) ExecutePostBuildAdaptations(logger, _usages);
        }

        public SerializableArtifact[] ExecutePreBuildAdaptations(ILogger logger)
        {
            return ExecutePreBuildAdaptationsInternal(logger);
        }

        public CompileItemReplacement[] ReplaceCompileItems(ILogger logger)
        {
            var result = new List<CompileItemReplacement>();
            logger.Debug("------------");
            logger.Info("Replacing source code files which contains adaptation usages");

            var intermediateDirectory = _explorer.Directories[BuildDirectory.Intermediate];

            var groupedUsages = _usages.Enumerate().ToLookup(u => u.SyntaxTree.FilePath, u => u);
            logger.Info($"{groupedUsages.Count(g => g.Any(u => !u.Escaped))} files will be replaced");

            var pattern = @"\[(assembly:)?(module:)?\s*\]";
            var options = RegexOptions.IgnorePatternWhitespace;
            var regex = new Regex(pattern, options);

            foreach (var group in groupedUsages)
            {
                var sourceFile = group.Key;

                if (group.All(u => u.Escaped))
                {
                    logger.Info($"All adaptation are isolated in {sourceFile} file");
                    continue;
                }

                if (group.Any(u => u.Escaped))
                {
                    var warnMessage = $"Mixed isolation detected in {sourceFile} file. Unescaped adaptations:" + Environment.NewLine;
                    foreach (var unescapedSyntax in group.Where(u => !u.Escaped))
                    {
                        var location = unescapedSyntax.SyntaxTree
                                                      .GetMappedLineSpan(unescapedSyntax.AttributeData.ApplicationSyntaxReference.Span)
                                                      .Span;
                        warnMessage += $"  Position {location}: {unescapedSyntax.AttributeData}{Environment.NewLine}";
                    }

                    logger.Warn(warnMessage);
                }

                var replacedFile = Path.Combine(intermediateDirectory, "__replacements", sourceFile.GetMD5Hash() + ".cs");

                logger.Debug("+ Replacement");
                logger.Debug("    Removed: " + sourceFile);
                logger.Debug("    Added: " + replacedFile);

                try
                {
                    var visitor = new CompileItemRewriterVisitor(group);
                    var syntaxTree = group.FirstOrDefault()?.SyntaxTree;
                    if (syntaxTree == null) continue;
                    var rewrittenItem = visitor.Visit(syntaxTree.GetRoot());

                    Path.GetDirectoryName(replacedFile)?.EnsureDirectoryExist();
                    var replacedFileContent = rewrittenItem.ToFullString();
                    replacedFileContent = regex.Replace(replacedFileContent, string.Empty);

                    File.WriteAllText(replacedFile, replacedFileContent);

                    var replacement = new CompileItemReplacement
                    {
                        Source = sourceFile,
                        Target = replacedFile
                    };

                    result.Add(replacement);
                }
                catch (Exception e)
                {
                    logger.Warn($"File '{sourceFile}' could not be replaced. Details: " + e.GetBaseException().Message);
                }
            }

            return result.ToArray();
        }

        private List<AdaptationUsage> AnalyzeSemanticForAdaptationUsages(SuspiciousAttributeSyntaxes suspiciousAttributeSyntaxes,
                                                                         CSharpCompilation compilation,
                                                                         ILogger logger)
        {
            logger.Debug("------------");
            logger.Info("Analyzing semantic");

            var suspiciousSyntaxTrees = suspiciousAttributeSyntaxes.ToLookup(pair => pair.Syntax.SyntaxTree, pair => pair);
            var usages = new List<AdaptationUsage>();
            foreach (var group in suspiciousSyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(group.Key, true);

                foreach (var syntax in group)
                {
                    try
                    {
                        var attributeInfo = semanticModel.GetSymbolInfo(syntax.Syntax);
                        var symbol = attributeInfo.Symbol ?? attributeInfo.CandidateSymbols.FirstOrDefault();
                        if (symbol == null) continue;

                        var resolvedType = syntax.PossibleTypes.FirstOrDefault(t => symbol.ResolveType() == t);
                        var attributeData = syntax.Syntax.ResolveAttributeData(semanticModel);
                        if (resolvedType == null) throw new InvalidDataException($"Could not resolve '{syntax.Syntax}' type");
                        if (attributeData?.Item2 == null) throw new InvalidDataException("Could not resolve attribute semantic");
                        if (attributeData.Item1 == AttributeTargets.All)
                            throw new InvalidOperationException("Unexpected AttributeTargets in resolved attribute data");

                        usages.Add(new AdaptationUsage(semanticModel,
                                                       group.Key,
                                                       syntax.Syntax.Parent.Parent,
                                                       attributeData.Item2,
                                                       attributeData.Item1,
                                                       resolvedType,
                                                       syntax.Escaped));
                    }
                    catch (NotSupportedException e)
                    {
                        logger.Debug($"Not supported feature. Details: {e.GetBaseException().Message}");
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Unexpected internal error. Details: {e.GetBaseException().Message}");
                    }
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
                                                                             IEnumerable<Tuple<SyntaxTree, SyntaxTree>> syntaxTrees,
                                                                             ILogger logger)
        {
            logger.Debug("------------");
            logger.Info("Analyzing syntax");

            var visitor = new SuspiciousAttributeVisitor(adaptationTypes);
            foreach (var syntaxTree in syntaxTrees)
            {
                visitor.IsChanged.Reset();
                visitor.Visit(syntaxTree.Item2.GetRoot());
                if (visitor.IsChanged.WaitOne(0))
                {
                    logger.Debug("Suspicious attributes isolation test");
                    visitor.Visit(syntaxTree.Item1.GetRoot());
                }
            }

            var suspiciousAttributeSyntaxes = visitor.SuspiciousAttributeSyntaxes;
            logger.Info($"Found {suspiciousAttributeSyntaxes.Count} suspicious attributes");
            foreach (var syntax in suspiciousAttributeSyntaxes)
            {
                logger.Debug($"+ Syntax: {syntax.Syntax}");
                using (logger.IndentMessages())
                {
                    logger.Debug($"Escaped: {syntax.Escaped}");
                    logger.Debug($"Location: {syntax.Syntax.GetLocation()}");
                    foreach (var type in syntax.PossibleTypes)
                    {
                        using (logger.IndentMessages())
                            logger.Debug($"* Could be: {type.FullName}");
                    }
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

        private List<Tuple<SyntaxTree, SyntaxTree>> CreateSyntaxTrees()
        {
            var syntaxTrees = _explorer.Items[BuildItem.Compile].Select(r =>
            {
                var text = File.ReadAllText(r.FullPath);

                var symbols = GetBuildConstants().ToList();

                var treeWithoutEscapes = CSharpSyntaxTree.ParseText(text,
                                                                    path: r.FullPath,
                                                                    options: new CSharpParseOptions(preprocessorSymbols: symbols));

                symbols.Add("ADAPTATION");
                var treeWithEscapes = CSharpSyntaxTree.ParseText(text,
                                                                 path: r.FullPath,
                                                                 options: new CSharpParseOptions(preprocessorSymbols: symbols));

                return new Tuple<SyntaxTree, SyntaxTree>(treeWithoutEscapes, treeWithEscapes);
            }).ToList();
            return syntaxTrees;
        }

        private void ExecutePostBuildAdaptations(ILogger logger, List<AdaptationUsage> usages)
        {
            if (usages.All(u => u.PostBuildMethod == null))
            {
                logger.Info("There is no discovered adaptations with post build instructions");
                return;
            }

            logger.Info("Executing discovered adaptations with post build instructions");

            foreach (var usage in Sort(usages))
            {
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
                        attribute = usage.AttributeData.CreateAttribute();
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Could not create adaptation attribute. Details: {e.GetBaseException().Message}.");
                        continue;
                    }
                }

                try
                {
                    logger.Info("------------");
                    logger.Info($"Adaptation: {usage.AttributeData}");

                    var serviceProvider = new ServiceProvider();
                    serviceProvider.AddService(typeof(CSharpCompilation), _compilation);
                    serviceProvider.AddService(typeof(SyntaxNode), usage.AssociatedSyntaxNode);
                    serviceProvider.AddService(typeof(SyntaxTree), usage.SyntaxTree);
                    serviceProvider.AddService(typeof(SemanticModel), usage.SemanticModel);
                    serviceProvider.AddService(typeof(ILogger), new ScopeLogger(logger));
                    serviceProvider.AddService(typeof(IExplorer), _explorer);
                    serviceProvider.AddService(typeof(INugetExplorer), _nugetExplorer);
                    serviceProvider.AddService(typeof(IDynamicVault), _dynamicVault);
                    serviceProvider.AddService(typeof(IMacroResolver), _macroResolver);

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

            foreach (var usage in Sort(usages))
            {
                var method = usage.PreBuildMethod;
                if (method == null)
                {
                    logger.Debug("Adaptation does not contain void PreBuild(IServiceProvider provider) method. Skipping...");
                    continue;
                }

                Attribute attribute;
                try
                {
                    attribute = usage.AttributeData.CreateAttribute();
                    usage.Attribute = attribute;
                }
                catch (Exception e)
                {
                    logger.Warn($"Could not create adaptation attribute. Details: {e.GetBaseException().Message}.");
                    continue;
                }

                try
                {
                    logger.Info("------------");
                    logger.Info($"Adaptation: {usage.AttributeData}");

                    var artifactory = new Artifactory();
                    var serviceProvider = new ServiceProvider();
                    serviceProvider.AddService(typeof(CSharpCompilation), _compilation);
                    serviceProvider.AddService(typeof(SyntaxNode), usage.AssociatedSyntaxNode);
                    serviceProvider.AddService(typeof(SyntaxTree), usage.SyntaxTree);
                    serviceProvider.AddService(typeof(SemanticModel), usage.SemanticModel);
                    serviceProvider.AddService(typeof(ILogger), new ScopeLogger(logger));
                    serviceProvider.AddService(typeof(IExplorer), _explorer);
                    serviceProvider.AddService(typeof(IArtifactory), artifactory);
                    serviceProvider.AddService(typeof(INugetExplorer), _nugetExplorer);
                    serviceProvider.AddService(typeof(IDynamicVault), _dynamicVault);
                    serviceProvider.AddService(typeof(IMacroResolver), _macroResolver);

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

        private SerializableArtifact[] ExecutePreBuildAdaptationsInternal(ILogger logger)
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

            _compilation = CreateCompilation(syntaxTrees.Select(t => t.Item2), logger);
            if (_compilation == null) throw new Exception("Can not create compilation");

            _usages = AnalyzeSemanticForAdaptationUsages(suspiciousAttributeSyntaxes, _compilation, logger);

            if (!_usages.Any())
            {
                logger.Info("Assembly does not use any defined adaptation attribute.");
                return result.ToArray();
            }

            var setupMethods = new List<MethodInfo>();
            foreach (var usage in Sort(_usages))
            {
                setupMethods.AddRange(usage.SetupMethods);
            }

            if (setupMethods.Any())
            {
                logger.Info("Setup adaptations");
                var calledMethods = new List<MethodInfo>();
                foreach (var method in setupMethods)
                {
                    if (calledMethods.Contains(method)) continue;
                    calledMethods.Add(method);

                    logger.Info("------------");
                    logger.Info($"Setup: {method.DeclaringType?.Name} type");

                    var serviceProvider = new ServiceProvider();
                    serviceProvider.AddService(typeof(ILogger), new ScopeLogger(logger));
                    serviceProvider.AddService(typeof(IExplorer), _explorer);
                    serviceProvider.AddService(typeof(INugetExplorer), _nugetExplorer);
                    serviceProvider.AddService(typeof(IDynamicVault), _dynamicVault);
                    serviceProvider.AddService(typeof(IMacroResolver), _macroResolver);
                    method.Invoke(null, new object[] { serviceProvider });
                }
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

        private string[] GetBuildConstants()
        {
            var constants = _explorer.Properties[BuildProperty.DefineConstants] ?? string.Empty;
            return constants.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
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
                    var assembly = _assemblyResolver.LoadAssembly(reference);
                    var foundTypes = assembly.GetTypesSafely()
                                             .Where(t => typeof(Attribute).IsAssignableFrom(t))
                                             .Where(t => t.GetCustomAttribute<DesignerAttribute>()?.DesignerTypeName == "PS.Build.Adaptation")
                                             .ToList();

                    adaptationDefinitionTypes.AddRange(foundTypes);
                }
                catch (Exception e)
                {
                    logger.Warn($"Could not load reference assembly '{reference}'. Details: {e.GetBaseException().Message}");
                }
            }

            //Recheck attributes for inheritance from adaptation types
            var adaptationTypes = AppDomain.CurrentDomain
                                           .GetAssemblies()
                                           .Where(a => pathBanns.Any(p => a.Location.Contains(p) != true))
                                           .SelectMany(
                                               a => a.GetTypesSafely().Where(t => adaptationDefinitionTypes.Any(adf => adf.IsAssignableFrom(t))))
                                           .Where(t => !t.IsAbstract)
                                           .ToList();

            //Check attribute usage inheritance option
            var attributesWithWrongInheritance = adaptationTypes.Where(t =>
            {
                var usageAttribute = t.GetCustomAttribute<AttributeUsageAttribute>() ??
                                     new AttributeUsageAttribute(AttributeTargets.All);
                if (usageAttribute.ValidOn.HasFlag(AttributeTargets.Method) ||
                    usageAttribute.ValidOn.HasFlag(AttributeTargets.Class)) return usageAttribute.Inherited;
                return false;
            }).ToList();
            attributesWithWrongInheritance.ForEach(type => logger.Warn($"{type.FullName} has invalid inheritance attribute. Skipping..."));
            adaptationTypes = adaptationTypes.Except(attributesWithWrongInheritance).ToList();

            //Check attributes with out key methods
            var emptyAttributes = adaptationTypes.Where(t => AdaptationUsage.GetPreBuildMethod(t) == null &&
                                                             AdaptationUsage.GetPostBuildMethod(t) == null &&
                                                             !AdaptationUsage.GetSetupMethods(t).Any())
                                                 .ToList();
            emptyAttributes.ForEach(type => logger.Warn($"{type.FullName} has no Setup, PreBuid or PostBuild entries. Skipping..."));
            adaptationTypes = adaptationTypes.Except(emptyAttributes).ToList();

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