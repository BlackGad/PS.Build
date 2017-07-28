using System.IO;
using System.Runtime.CompilerServices;
using PS.Build.Services;
using PS.Build.Tasks.Services;

namespace PS.Build.Tasks.Tests
{
    public static class ProjectBeacon
    {
        #region Static members

        public static string GetNugetToolPath()
        {
            INugetExplorer nugetExplorer = new NugetExplorer(GetSolutionDirectory());
            return Path.Combine(nugetExplorer.FindPackage("NuGet.CommandLine").Folder, @"tools\NuGet.exe");
        }

        public static string GetSolutionDirectory()
        {
            var currentFilePath = GetCurrentFilePath();
            // ReSharper disable once AssignNullToNotNullAttribute
            return Path.Combine(Path.GetDirectoryName(currentFilePath), @"..\");
        }

        private static string GetCurrentFilePath([CallerFilePath] string path = null)
        {
            return path;
        }

        #endregion
    }
}