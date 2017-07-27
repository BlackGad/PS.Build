using System.Collections.Generic;
using PS.Build.Services;
using PS.Build.Tasks.Extensions;

namespace PS.Build.Tasks.Services
{
    class DynamicVault : IDynamicVault
    {
        private readonly Dictionary<object, object> _storage;

        #region Constructors

        public DynamicVault()
        {
            _storage = new Dictionary<object, object>();
        }

        #endregion

        #region IDynamicVault Members

        public bool Query(object key, out object value)
        {
            if (_storage.ContainsKey(key))
            {
                value = _storage[key];
                return true;
            }

            value = null;
            return false;
        }

        public object Store(object key, object value)
        {
            return _storage.Set(key, () => value);
        }

        #endregion
    }
}