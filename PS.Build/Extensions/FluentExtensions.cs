using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Services;

namespace PS.Build.Extensions
{
    public static class FluentExtensions
    {
        #region Static members

        public static T GetService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }

        public static T Query<T>(this IDynamicVault vault, Func<T> createFactory = null)
        {
            return vault.Query<T>(null, createFactory);
        }

        public static T Query<T>(this IDynamicVault vault, string vaultKey, Func<T> createFactory = null)
        {
            T result = default(T);
            object vaultData;
            var key = typeof(T).FullName + vaultKey;
            if (vault.Query(key, out vaultData))
            {
                result = (T)vaultData;
                return result;
            }

            if (createFactory != null) result = createFactory();
            vault.Store(key, result);

            return result;
        }

        public static string Resolve(this IMacroResolver resolver, string source)
        {
            ValidationResult[] errors;
            return resolver.Resolve(source, out errors);
        }

        public static T Store<T>(this IDynamicVault vault, T value)
        {
            return vault.Store<T>(null, value);
        }

        public static T Store<T>(this IDynamicVault vault, string vaultKey, T value)
        {
            return (T)vault.Store(typeof(T).FullName + vaultKey, value);
        }

        #endregion
    }
}