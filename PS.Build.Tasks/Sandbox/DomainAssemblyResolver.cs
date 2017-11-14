using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PS.Build.Extensions;
using PS.Build.Services;

namespace PS.Build.Tasks
{
    class DomainAssemblyResolver : MarshalByRefObject,
                                   IDisposable
    {
        private readonly ConcurrentDictionary<string, Assembly> _cache;

        private readonly List<string> _directoriesToScan;
        private readonly ILogger _logger;

        private readonly string _temporaryDirectory;

        #region Constructors

        public DomainAssemblyResolver(string[] directoriesToScan, ILogger logger)
        {
            if (directoriesToScan == null) throw new ArgumentNullException(nameof(directoriesToScan));
            _temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")).EnsureSlash();
            //_temporaryDirectory = @"e:\temp\refs\";

            _cache = new ConcurrentDictionary<string, Assembly>();
            _logger = logger;
            _directoriesToScan = new List<string>(directoriesToScan.Select(d => d.ToLowerInvariant().EnsureSlash()));
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        #endregion

        #region Event handlers

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly result;
            if (_cache.TryGetValue(args.Name, out result)) return result;

            var pattern = @"^(?<name>[^,]+),[ ]*Version=(?<version>[^,]+)(,[ ]*Culture=(?<culture>[^,]+))?(,[ ]*PublicKeyToken=(?<token>[^,]+))?$";
            var matchResult = Regex.Match(args.Name, pattern);
            if (!matchResult.Success) return null;

            var assemblyName = matchResult.Groups["name"].Value;
            Version assemblyVersion;
            Version.TryParse(matchResult.Groups["version"].Value, out assemblyVersion);

            var expectedAssemblyDirectory = Path.Combine(_temporaryDirectory, assemblyName).EnsureSlash();
            var expectedAssemblyPath = Path.Combine(expectedAssemblyDirectory, assemblyVersion.ToString(), assemblyName + ".dll");

            string resolvedPath = null;
            if (File.Exists(expectedAssemblyPath)) resolvedPath = expectedAssemblyPath;
            else
            {
                foreach (var directory in _directoriesToScan)
                {
                    var possibleAssemblyPath = Path.Combine(directory, assemblyName + ".dll");
                    if (!File.Exists(possibleAssemblyPath)) continue;

                    CacheAssembly(possibleAssemblyPath);
                }
            }

            if (File.Exists(expectedAssemblyPath)) resolvedPath = expectedAssemblyPath;
            else
            {
                var versions = IOExtensions.EnumerateDirectories(expectedAssemblyDirectory + "\\*")
                                           .Select(d =>
                                           {
                                               d = d.TrimEnd('\\');
                                               Version v;
                                               Version.TryParse(d.Substring(d.LastIndexOf('\\') + 1), out v);
                                               return v;
                                           })
                                           .Where(v => v != null)
                                           .OrderByDescending(r => r).ToList();

                if (versions.Any()) resolvedPath = Path.Combine(expectedAssemblyDirectory, versions.First().ToString(), assemblyName + ".dll");
            }

            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                _logger.Warn($"# Assembly {args.Name} not resolved");
                _cache.TryAdd(args.Name, null);
                return null;
            }

            _logger.Info($"# Assembly {args.Name} resolved with {resolvedPath}");

            result = Assembly.LoadFile(resolvedPath);
            _cache.TryAdd(args.Name, result);

            return result;
        }

        #endregion

        #region Members

        public Assembly LoadAssembly(string path)
        {
            var fileDirectory = Path.GetDirectoryName(path)?.ToLowerInvariant().EnsureSlash();
            if (fileDirectory == null) return null;

            if (_directoriesToScan.All(d => d != fileDirectory)) _directoriesToScan.Add(fileDirectory);

            return Assembly.LoadFile(CacheAssembly(path));
        }

        private string CacheAssembly(string path)
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(path);

                var cachedAssemblyDirectory = Path.Combine(_temporaryDirectory, assemblyName.Name, assemblyName.Version.ToString()).EnsureSlash();
                var cachedAssemblyPath = Path.Combine(cachedAssemblyDirectory, assemblyName.Name + ".dll");

                if (File.Exists(cachedAssemblyPath)) return cachedAssemblyPath;
                cachedAssemblyDirectory.EnsureDirectoryExist();
                File.Copy(path, cachedAssemblyPath);
                if (File.Exists(path + ".config")) File.Copy(path + ".config", cachedAssemblyPath + ".config");
                return cachedAssemblyPath;
            }
            catch (Exception e)
            {
                _logger.Debug($"Could not cache assembly {path}. Details: " + e.GetBaseException().Message);
                return null;
            }
        }

        #endregion
    }
}