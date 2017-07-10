using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using PS.Build.Services;
using PS.Build.Tasks.Extensions;

namespace PS.Build.Tasks
{
    public class CacheManager<TPayload> : IDisposable where TPayload : class
    {
        private readonly string _intermediatePath;
        private readonly ILogger _logger;

        private Dictionary<int, CacheRecord<TPayload>> _cacheTable;

        #region Constructors

        public CacheManager(string intermediatePath, ILogger logger)
        {
            if (intermediatePath == null) throw new ArgumentNullException(nameof(intermediatePath));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _intermediatePath = intermediatePath;
            _logger = logger;
        }

        #endregion

        #region Properties

        private Dictionary<int, CacheRecord<TPayload>> CacheTable
        {
            get
            {
                if (_cacheTable != null) return _cacheTable;
                var cachePath = GetCachePath();
                if (File.Exists(cachePath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(cachePath))
                        {
                            var formatter = new BinaryFormatter();
                            var cacheArray = (CacheRecord<TPayload>[])formatter.Deserialize(stream);
                            _cacheTable = cacheArray.ToDictionary(r => r.Key, r => r);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Warn($"Cache index '{cachePath}' exist but failed to load. Details: {e.Message}");
                    }
                }

                return _cacheTable ?? (_cacheTable = new Dictionary<int, CacheRecord<TPayload>>());
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                var cachePath = GetCachePath();
                Path.GetDirectoryName(cachePath).EnsureDirectoryExist();

                using (var stream = File.OpenWrite(cachePath))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, CacheTable.Values.ToArray());
                }
            }
            catch (Exception)
            {
                //Cache failed to save
            }
        }

        #endregion

        #region Members

        public void Cache(string path, TPayload payload)
        {
            try
            {
                var cacheTable = CacheTable;
                var key = path.GetHashCode();
                if (!cacheTable.ContainsKey(key)) cacheTable.Add(key, new CacheRecord<TPayload>());

                cacheTable[key].Key = key;
                cacheTable[key].Payload = payload;
                _logger.Debug($"+ Cached: {path}");
            }
            catch (Exception e)
            {
                _logger.Warn($"{path} was not added to cache. Details: {e.Message}");
            }
        }

        public TPayload GetCached(string path)
        {
            var hashCode = path.GetHashCode();
            if (!CacheTable.ContainsKey(hashCode)) return null;
            return CacheTable[hashCode].Payload;
        }

        public string GetCachePath()
        {
            return Path.Combine(_intermediatePath, $"_{typeof(TPayload).Name}.cache");
        }

        #endregion

        #region Nested type: CacheRecord

        [Serializable]
        class CacheRecord<TRecordPayload>
        {
            #region Properties

            public int Key { get; set; }

            public TRecordPayload Payload { get; set; }

            #endregion
        }

        #endregion
    }
}