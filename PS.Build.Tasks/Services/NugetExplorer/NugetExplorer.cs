using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using PS.Build.Services;
using PS.Build.Tasks.Extensions;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class NugetExplorer : INugetExplorer
    {
        #region Constants

        const string ConfigFilename = "NuGet.Config";
        const string DefaultDirectory = "packages";
        const string StandardXPath = "/configuration/config/add[@key='repositoryPath']";

        const string VersionPattern =
            @"(?<id>^.+?(?=[\._]\d+[\._]\d+([\._]\d+[\._]\d+)?))[\._](?<major>\d+)[\._](?<minor>\d+)([\._](?<build>\d+)([\._](?<revision>\d+))?)?";

        #endregion

        #region Static members

        private static INugetPackage GetLattestNugetPackage(string packageId, string nugetDirectory)
        {
            if (nugetDirectory == null) throw new ArgumentException("Illegal archive directory");

            var regexMask = new Regex((packageId + ".*").Replace(".", "\\.").Replace("*", ".*"),
                                      RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var regexVersion = new Regex(VersionPattern);

            var files = new Dictionary<Version, string>();
            foreach (var directory in Directory.EnumerateDirectories(nugetDirectory))
            {
                if (!regexMask.IsMatch(directory)) continue;

                var versionMatch = regexVersion.Match(directory);

                var id = versionMatch.Groups["id"].ToString();
                if (!id.EndsWith(packageId, StringComparison.InvariantCultureIgnoreCase)) continue;
                var major = versionMatch.Groups["major"].ToString();
                var minor = versionMatch.Groups["minor"].ToString();
                var build = versionMatch.Groups["build"].ToString();
                var revision = versionMatch.Groups["revision"].ToString();
                major = string.IsNullOrEmpty(major) ? "0" : major;
                minor = string.IsNullOrEmpty(minor) ? "0" : minor;
                build = string.IsNullOrEmpty(build) ? "0" : build;
                revision = string.IsNullOrEmpty(revision) ? "0" : revision;

                files.Ensure(new Version($"{major}.{minor}.{build}.{revision}"), () => directory);
            }

            var pair = files.Any()
                ? files.OrderByDescending(f => f.Key).First()
                : (KeyValuePair<Version, string>?)null;

            if (pair.HasValue)
            {
                var directory = pair.Value.Value.NormalizePath().EnsureSlash();
                var version = pair.Value.Key;
                return new NugetPackage(packageId, version, directory);
            }
            return null;
        }

        #endregion

        private readonly string _nugetDirectory;

        #region Constructors

        public NugetExplorer(string solutionDirectory)
        {
            var possibleConfigFilePath = Path.Combine(solutionDirectory, ConfigFilename);
            string nugetPackageDirectory = null;

            try
            {
                if (File.Exists(possibleConfigFilePath))
                {
                    var config = XDocument.Load(possibleConfigFilePath);

                    var repositoryNode = ((IEnumerable)config.XPathEvaluate(StandardXPath)).OfType<XElement>().FirstOrDefault();
                    if (repositoryNode != null) nugetPackageDirectory = repositoryNode.Attribute("value")?.Value ?? DefaultDirectory;
                }
            }
            catch (Exception)
            {
                //Nothing
            }

            nugetPackageDirectory = nugetPackageDirectory ?? DefaultDirectory;
            _nugetDirectory = Path.Combine(solutionDirectory, nugetPackageDirectory).NormalizePath().EnsureSlash();
        }

        #endregion

        #region INugetExplorer Members

        INugetPackage INugetExplorer.FindPackage(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId)) throw new ArgumentException("Invalid Nuget package id");
            return GetLattestNugetPackage(packageId, _nugetDirectory);
        }

        #endregion
    }
}