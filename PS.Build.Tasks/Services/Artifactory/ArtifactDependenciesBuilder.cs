using System.Collections.Generic;
using System.IO;
using System.Linq;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    public class ArtifactDependenciesBuilder : IArtifactDependenciesBuilder
    {
        private readonly List<string> _fileDependencies;
        private readonly List<string> _tagDependencies;

        #region Constructors

        public ArtifactDependenciesBuilder()
        {
            _fileDependencies = new List<string>();
            _tagDependencies = new List<string>();
        }

        #endregion

        #region IArtifactDependenciesBuilder Members

        public IArtifactDependenciesBuilder FileDependency(string path)
        {
            _fileDependencies.Add(path ?? string.Empty);
            return this;
        }

        public IArtifactDependenciesBuilder TagDependency(string tag)
        {
            _tagDependencies.Add(tag ?? string.Empty);
            return this;
        }

        #endregion

        #region Members

        public IEnumerable<int> GetDependenciesHashCodes()
        {
            foreach (var fileDependency in _fileDependencies)
            {
                var file = new FileInfo(fileDependency);
                var tags = new List<string>
                {
                    $"File: {file.FullName.ToLowerInvariant()}"
                };

                if (file.Exists)
                {
                    tags.Add($"WriteTime: {file.LastWriteTime}");
                    tags.Add($"Lenght: {file.Length}");
                }

                yield return tags.Aggregate(0, (agg, d) => (agg*397) ^ d.GetHashCode());
            }

            foreach (var tagDependency in _tagDependencies)
            {
                yield return tagDependency.GetHashCode();
            }
        }

        #endregion
    }
}