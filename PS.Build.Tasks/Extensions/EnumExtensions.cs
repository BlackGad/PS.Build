using System;

namespace PS.Build.Tasks.Extensions
{
    /// <summary>
    ///     EnumExtensions class provides method(s) which extends native .net method(s) to work with Enums.
    /// </summary>
    public static class EnumExtensions
    {
        #region Static members

        /// <summary>
        ///     Checks Enum variable for flagged enum value entrance.
        /// </summary>
        /// <param name="variable">Enum variable.</param>
        /// <param name="value">Flagged enum value.</param>
        /// <returns>True if variable contains flag; False otherwise.</returns>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            if (variable == null) return false;
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(
                    $"Enumeration type mismatch. The flag is of type '{value.GetType()}', was expecting '{variable.GetType()}'.");
            }

            var num = Convert.ToUInt64(value);
            return (Convert.ToUInt64(variable) & num) == num;
        }

        #endregion
    }
}