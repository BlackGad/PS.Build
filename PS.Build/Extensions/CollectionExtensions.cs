using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PS.Build.Extensions
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

        /// <summary>
        ///     Enumerates <paramref name="object" /> as generic enumeration and cast it to target type.
        /// </summary>
        /// <param name="object">Source object.</param>
        /// <returns>
        ///     If <paramref name="object" /> is <see cref="IEnumerable" /> and enumeration is null returns empty enumeration,
        ///     source enumeration otherwise.
        /// </returns>
        public static IEnumerable<T> Enumerate<T>(this object @object)
        {
            var enumerable = @object as IEnumerable;
            return enumerable?.OfType<T>() ?? Enumerable.Empty<T>();
        }

        /// <summary>
        ///     Enumerates enumeration.
        /// </summary>
        /// <param name="enumerable">IEnumerable object.</param>
        /// <returns>If enumeration is null returns empty enumeration, source enumeration otherwise.</returns>
        public static IEnumerable Enumerate(this IEnumerable enumerable)
        {
            return enumerable ?? Enumerable.Empty<object>();
        }

        /// <summary>
        ///     Enumerates enumeration.
        /// </summary>
        /// <param name="enumerable">IEnumerable object.</param>
        /// <returns>If enumeration is null returns empty enumeration, source enumeration otherwise.</returns>
        public static IEnumerable<T> Enumerate<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        /// <summary>
        ///     Enumerates <paramref name="object" /> as generic enumeration.
        /// </summary>
        /// <param name="object">Source object.</param>
        /// <returns>
        ///     If <paramref name="object" /> is <see cref="IEnumerable" /> and enumeration is null returns empty enumeration,
        ///     source enumeration otherwise.
        /// </returns>
        public static IEnumerable<object> Enumerate(this object @object)
        {
            var enumerable = @object as IEnumerable;
            return enumerable.Enumerate<object>();
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