using System;
using System.Collections.Generic;
using System.Linq;
using PS.Build.Extensions;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class Artifact : IArtifactBuilder
    {
        #region Static members

        public static bool operator ==(Artifact left, Artifact right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Artifact left, Artifact right)
        {
            return !Equals(left, right);
        }

        #endregion

        private readonly Dictionary<string, string> _metadata;
        private readonly BuildItem _type;
        private bool _isPermanent;

        #region Constructors

        internal Artifact(string path, BuildItem type)
        {
            _metadata = new Dictionary<string, string>();
            Path = path;
            _type = type;
            Dependencies = new ArtifactDependenciesBuilder();
        }

        #endregion

        #region Properties

        public Func<byte[]> ContentFactory { get; private set; }

        public ArtifactDependenciesBuilder Dependencies { get; }
        public string Path { get; }

        #endregion

        #region Override members

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Artifact)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Path?.GetHashCode() ?? 0)*397) ^ (int)_type;
            }
        }

        public override string ToString()
        {
            return $"({_type}) {Path}";
        }

        #endregion

        #region IArtifactBuilder Members

        IArtifactBuilder IArtifactBuilder.Content(Func<byte[]> contentFactory)
        {
            if (contentFactory == null) throw new ArgumentNullException(nameof(contentFactory));
            ContentFactory = contentFactory;
            return this;
        }

        IArtifactDependenciesBuilder IArtifactBuilder.Dependencies()
        {
            return Dependencies;
        }

        IArtifactBuilder IArtifactBuilder.Metadata(string type, string value)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (Item.ReservedPropertyNames.Contains(type)) throw new ArgumentException($"{type} type is reserved.");
            _metadata.Set(type, () => value ?? string.Empty);
            return this;
        }

        IArtifactBuilder IArtifactBuilder.Permanent()
        {
            _isPermanent = true;
            return this;
        }

        #endregion

        #region Members

        public SerializableArtifact Serialize()
        {
            return new SerializableArtifact
            {
                Path = Path,
                Metadata = _metadata,
                Type = _type,
                IsPermanent = _isPermanent,
            };
        }

        protected bool Equals(Artifact other)
        {
            return string.Equals(Path, other.Path) && _type == other._type;
        }

        #endregion
    }
}