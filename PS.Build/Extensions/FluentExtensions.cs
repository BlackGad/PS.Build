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

        public static T Query<T>(this IDynamicVault vault)
        {
            object value;
            if (vault.Query(typeof(T), out value)) return (T)value;
            return default(T);
        }

        public static string Resolve(this IMacroResolver resolver, string source)
        {
            ValidationResult[] errors;
            return resolver.Resolve(source, out errors);
        }

        public static T Store<T>(this IDynamicVault vault, T value)
        {
            return (T)vault.Store(typeof(T), value);
        }

        #endregion
    }
}