using System;
using System.Collections.Generic;

namespace PS.Build.Tasks.Extensions
{
    public static class CollectionExtensions
    {
        #region Static members

        public static TValue Ensure<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, factory());
            return dictionary[key];
        }

        public static TValue Get<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory = null)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (dictionary.ContainsKey(key)) return dictionary[key];
            if (factory != null) return factory();
            return default(TValue);
        }

        public static TValue Set<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, factory());
            else dictionary[key] = factory();

            return dictionary[key];
        }

        #endregion
    }
}