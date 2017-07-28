using System.ComponentModel.DataAnnotations;
using PS.Build.Types;

namespace PS.Build.Services
{
    /// <summary>
    ///     Macro resolver service. Add possibility to resolve macro in strings.
    /// </summary>
    public interface IMacroResolver
    {
        #region Members

        /// <summary>
        ///     Register macro handler.
        /// </summary>
        /// <param name="handler">Processor that could handle some of input macro</param>
        void Register(IMacroHandler handler);

        /// <summary>
        ///     Resolve input string macro
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="errors">The errors that occurred during the input string processing</param>
        /// <returns></returns>
        string Resolve(string source, out ValidationResult[] errors);

        #endregion
    }
}