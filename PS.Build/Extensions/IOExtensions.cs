using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PS.Build.Types;

namespace PS.Build.Extensions
{
    public static class IOExtensions
    {
        #region Constants

        private static readonly Regex RecursivePathRegex =
            new Regex(@"^(?<prefix>.*?(?=[\\/][^\\/]*\*))?(?<recursive>.*(?<=\*)(?=[\\/]))?(?<postfix>.*)$");

        #endregion

        #region Static members

        public static RecursivePath DetermineRecursivePath(this string filePath, string pattern)
        {
            var patternRecursive = string.Empty;
            var patternPrefix = string.Empty;
            var patternPostfix = string.Empty;

            var recursive = string.Empty;
            var prefix = string.Empty;
            var postfix = string.Empty;

            filePath = filePath.NormalizePath();
            pattern = pattern.NormalizePath();

            var match = RecursivePathRegex.Match(pattern);
            if (!match.Success) return null;

            if (match.Groups["prefix"].Success)
            {
                patternPrefix = match.Groups["prefix"].Value.Trim('\\');
                prefix = patternPrefix;
            }
            if (match.Groups["postfix"].Success)
            {
                patternPostfix = match.Groups["postfix"].Value.Trim('\\');
                var postfixLevels = patternPostfix.Occurrences('\\') + 1;
                var postfixStartPosition = filePath.Length;
                for (var i = 0; i < postfixLevels; i++)
                {
                    postfixStartPosition = filePath.LastIndexOf("\\", postfixStartPosition - 1, StringComparison.InvariantCultureIgnoreCase);
                }
                if (postfixStartPosition == -1) postfixStartPosition = 0;
                postfix = filePath.Substring(postfixStartPosition, filePath.Length - postfixStartPosition).Trim('\\');
            }
            if (match.Groups["recursive"].Success)
            {
                patternRecursive = match.Groups["recursive"].Value.Trim('\\');
                recursive = filePath.Substring(prefix.Length, filePath.Length - prefix.Length - postfix.Length).Trim('\\');
            }

            return new RecursivePath
            {
                PatternPrefix = patternPrefix,
                PatternPostfix = patternPostfix,
                PatternRecursive = patternRecursive,
                Prefix = prefix,
                Postfix = postfix,
                Recursive = recursive,
                Original = filePath
            };
        }

        public static string EnsureBackSlash(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return path.TrimEnd('/') + "/";
        }

        public static string EnsureDirectoryExist(this string directory)
        {
            if (directory == null) return null;
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return directory;
        }

        public static string EnsureSlash(this string path)
        {
            if (path == null) return null;
            return path.TrimEnd('\\') + "\\";
        }

        public static bool IsAbsolutePath(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return new Uri(path, UriKind.RelativeOrAbsolute).IsAbsoluteUri;
        }

        /// <summary>
        ///     Filter files by specified wildcard pattern.
        /// </summary>
        /// <param name="files">Source files.</param>
        /// <param name="sourcePattern">Wildcard pattern.</param>
        /// <param name="getter">File path getter</param>
        /// <returns>Files that match wildcard pattern.</returns>
        public static IEnumerable<T> Match<T>(IEnumerable<T> files, string sourcePattern, Func<T, string> getter = null)
        {
            files = files ?? Enumerable.Empty<T>();
            getter = getter ?? (arg => arg.ToString());

            Regex fileRegex;
            try
            {
                var searchPattern = Path.GetFileName(sourcePattern);
                fileRegex = new Regex("^" + ConvertWildcardToRegex(searchPattern) + "$", RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return Enumerable.Empty<T>();
            }

            Regex directoryRegex;
            try
            {
                var searchPath = Path.GetDirectoryName(sourcePattern);
                directoryRegex = new Regex((Path.IsPathRooted(sourcePattern) ? "^" : string.Empty) + ConvertWildcardToRegex(searchPath) + "$",
                                           RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                directoryRegex = null;
            }

            return files.Where(f =>
            {
                string filename;
                var path = getter(f);
                try
                {
                    filename = Path.GetFileName(path) ?? string.Empty;
                }
                catch (Exception)
                {
                    filename = null;
                }

                string directory;
                try
                {
                    directory = Path.GetDirectoryName(path) ?? string.Empty;
                }
                catch (Exception)
                {
                    directory = null;
                }

                var fileComparison = filename != null && fileRegex.IsMatch(filename);

                var directoryComparison = true;
                if (directoryRegex != null && directory != null) directoryComparison = directoryRegex.IsMatch(directory);

                return directoryComparison && fileComparison;
            });
        }

        public static string NormalizePath(this object url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            var uri = url as Uri;
            if (uri == null)
            {
                if (!Uri.TryCreate(url.ToString(), UriKind.RelativeOrAbsolute, out uri))
                {
                    throw new ArgumentException($"{url} is invalid uri");
                }
            }

            if (!uri.IsAbsoluteUri)
            {
                var currentDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
                uri = new Uri(new Uri(currentDirectory), uri);
            }

            var path = uri.LocalPath.Replace("*", "^@^");
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace("^@^", "*");
        }

        /// <summary>
        ///     Removes all empty directories recursively
        /// </summary>
        /// <param name="root">Directory to start</param>
        public static bool RemoveDirectoryIfEmpty(this string root)
        {
            if (string.IsNullOrEmpty(root)) return false;
            try
            {
                var entries = Directory.EnumerateFileSystemEntries(root);
                if (entries.Any()) return false;

                try
                {
                    Directory.Delete(root);
                    return true;
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
            catch (Exception)
            {
                //Nothing
            }

            return false;
        }

        /// <summary>
        ///     Removes all empty directories recursively from recursive part
        /// </summary>
        /// <param name="file">Recursive part</param>
        public static void RemoveEmptyDirectories(this RecursivePath file)
        {
            var prefix = file.Prefix;
            if (string.IsNullOrWhiteSpace(prefix)) return;
            prefix = prefix.TrimEnd('\\');

            var currentDirectory = Path.GetDirectoryName(file.Original)?.TrimEnd('\\');
            do
            {
                if (!currentDirectory.RemoveDirectoryIfEmpty()) break;

                var slashIndex = currentDirectory?.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
                if (slashIndex == null || slashIndex == -1) break;
                currentDirectory = currentDirectory.Substring(0, slashIndex.Value);
            } while (!string.IsNullOrWhiteSpace(prefix) && currentDirectory.Length > prefix.Length);
        }

        /// <summary>
        ///     Removes all empty directories recursively from root
        /// </summary>
        /// <param name="root">Directory to start</param>
        public static void RemoveEmptyDirectories(this string root)
        {
            if (string.IsNullOrEmpty(root)) return;

            foreach (var d in Directory.EnumerateDirectories(root))
            {
                RemoveEmptyDirectories(d);
            }

            root.RemoveDirectoryIfEmpty();
        }

        /// <summary>
        ///     Enumerates directories by pattern.
        /// </summary>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>Existing directories.</returns>
        public static SearchResult<string> SearchDirectories(string pattern)
        {
            return SearchDirectories(pattern, (pat, dir) => dir);
        }

        /// <summary>
        ///     Enumerates directories by pattern.
        /// </summary>
        /// <param name="pattern">Search pattern.</param>
        /// <param name="itemFactory">Search item factory</param>
        /// <returns>Existing directories.</returns>
        public static SearchResult<T> SearchDirectories<T>(string pattern, Func<string, string, T> itemFactory)
        {
            if (pattern == null) throw new InvalidOperationException();
            pattern = pattern.NormalizePath();

            var wildcardChars = new[] { '*', '?' };

            if (!Path.IsPathRooted(pattern)) pattern = Path.Combine(Directory.GetCurrentDirectory(), pattern);
            var containsWildcardChars = pattern.IndexOfAny(wildcardChars) >= 0;
            if (!containsWildcardChars && Directory.Exists(pattern))
                return new SearchResult<T>(pattern, new[] { itemFactory(pattern, pattern) });

            var pathRoot = Path.GetPathRoot(pattern);

            var subPathTokens = pattern.Substring(pathRoot.Length).Split('\\', '/').Where(p => !string.IsNullOrEmpty(p)).ToList();
            var wildcardTokenIndex = subPathTokens.FindIndex(p => p.Any(s => wildcardChars.Contains(s)));
            if (wildcardTokenIndex == -1) throw new InvalidDataException();

            var currentPath = Path.Combine(pathRoot, subPathTokens.Take(wildcardTokenIndex).Aggregate(string.Empty, Path.Combine));
            if (subPathTokens[wildcardTokenIndex] == "**")
            {
                var pathPostfix = string.Empty;
                var searchPattern = "*";

                if (subPathTokens.Count > wildcardTokenIndex + 1)
                    searchPattern = subPathTokens[wildcardTokenIndex + 1];

                if (subPathTokens.Count > wildcardTokenIndex + 2)
                    pathPostfix = subPathTokens.Skip(wildcardTokenIndex + 2).Aggregate(string.Empty, Path.Combine);

                var directoriesEnumeration = Directory.EnumerateDirectories(currentPath, searchPattern, SearchOption.AllDirectories)
                                                      .SelectMany(d => SearchDirectories(Path.Combine(d, pathPostfix), itemFactory).Items);

                if (string.IsNullOrEmpty(pathPostfix))
                    directoriesEnumeration = directoriesEnumeration.Union(new[] { itemFactory(pattern, currentPath) });

                return new SearchResult<T>(pattern, directoriesEnumeration);
            }
            else
            {
                var pathPostfix = string.Empty;
                if (subPathTokens.Count > wildcardTokenIndex + 1)
                    pathPostfix = subPathTokens.Skip(wildcardTokenIndex + 1).Aggregate(string.Empty, Path.Combine);

                var directories = Directory.EnumerateDirectories(currentPath, subPathTokens[wildcardTokenIndex], SearchOption.TopDirectoryOnly);
                return new SearchResult<T>(pattern, directories.SelectMany(d => SearchDirectories(Path.Combine(d, pathPostfix), itemFactory).Items));
            }
        }

        /// <summary>
        ///     Enumerates files by pattern.
        /// </summary>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>Existing files.</returns>
        public static SearchResult<string> SearchFiles(string pattern)
        {
            return SearchFiles(pattern, (p, f) => f);
        }

        /// <summary>
        ///     Search files by pattern.
        /// </summary>
        /// <param name="pattern">Search pattern.</param>
        /// <param name="itemFactory">Search item factory</param>
        /// <returns>Existing files.</returns>
        public static SearchResult<T> SearchFiles<T>(string pattern, Func<string, string, T> itemFactory)
        {
            try
            {
                pattern = pattern.NormalizePath();

                if (!Path.IsPathRooted(pattern)) pattern = Path.Combine(Directory.GetCurrentDirectory(), pattern);
                var patternFilename = Path.GetFileName(pattern);
                var searchPattern = string.IsNullOrEmpty(patternFilename) ? "*" : patternFilename;
                var searchPath = Path.GetDirectoryName(pattern);
                if (searchPath == null) throw new InvalidOperationException();

                var directoriesSearch = SearchDirectories(searchPath, (pat, dir) => dir);
                return new SearchResult<T>(pattern,
                                           directoriesSearch.Items
                                                            .SelectMany(d => Directory.EnumerateFiles(d, searchPattern))
                                                            .Select(f => itemFactory(pattern, f)));
            }
            catch (Exception)
            {
                return new SearchResult<T>();
            }
        }

        private static string ConvertWildcardToRegex(string wildcard)
        {
            var pattern = Regex.Escape(wildcard);
            pattern = pattern.Replace(@"\\\*\*\\", @".*");
            pattern = pattern.Replace(@"\\\*\*", @".*");
            pattern = pattern.Replace(@"\*\*", @".*");
            pattern = pattern.Replace(@"\*", @".+");
            pattern = pattern.Replace(@"\?", @".");
            return pattern;
        }

        #endregion
    }
}