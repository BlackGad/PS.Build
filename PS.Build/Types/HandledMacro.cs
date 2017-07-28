using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PS.Build.Types
{
    /// <summary>
    ///     Macro handler handle result object.
    /// </summary>
    public class HandledMacro
    {
        #region Constructors

        /// <summary>
        ///     Construct object with successful result.
        /// </summary>
        /// <param name="result">Successfully processed input string.</param>
        public HandledMacro(string result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            Result = result;
        }

        /// <summary>
        ///     Construct object with failed results.
        /// </summary>
        /// <param name="validationErrors">The errors that occurred during the input string processing</param>
        public HandledMacro(params ValidationResult[] validationErrors)
        {
            if (validationErrors?.Any(e => e != null) != true) throw new ArgumentException("Error is not specified");
            ValidationErrors = validationErrors;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Successful result string
        /// </summary>
        public string Result { get; }

        /// <summary>
        ///     Failed result errors
        /// </summary>
        public ValidationResult[] ValidationErrors { get; }

        #endregion
    }
}