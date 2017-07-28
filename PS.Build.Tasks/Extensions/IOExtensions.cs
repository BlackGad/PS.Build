using System;
using System.IO;

namespace PS.Build.Tasks.Extensions
{
    internal static class IOExtensions
    {
        #region Static members

        public static string EnsureDirectoryExist(this string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return directory;
        }

        public static string EnsureSlash(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return path.TrimEnd('\\') + "\\";
        }

        public static bool IsAbsolutePath(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return new Uri(path, UriKind.RelativeOrAbsolute).IsAbsoluteUri;
        }

        public static string NormalizePath(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        #endregion
    }
}