using System;

namespace PS.Build.Services
{
    /// <summary>
    ///     Logger provider service.
    /// </summary>
    public interface ILogger
    {
        #region Properties

        /// <summary>
        ///     Gets a value that indicates whether the task has logged any errors through this logging helper object.
        /// </summary>
        /// <returns>
        ///     true if the task has logged any errors through this logging helper object; otherwise, false.
        /// </returns>
        bool HasErrors { get; }

        #endregion

        #region Members

        /// <summary>
        ///     Log critical error. Break MSBuild build process on task end. Will produce error to build output.
        /// </summary>
        /// <param name="message"></param>
        void Critical(string message);

        /// <summary>
        ///     Log debug message. Message is visible only for detailed and diagnostic log level.
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);

        /// <summary>
        ///     Log generic error. Break MSBuild build process on task end. Will produce error to build output.
        /// </summary>
        void Error(string message);

        /// <summary>
        ///     Adds indent to Info and Debug messages after call. Removes indent after Dispose.
        /// </summary>
        /// <returns>Indent controller.</returns>
        IDisposable IndentMessages();

        /// <summary>
        ///     Log generic build output message.
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);

        /// <summary>
        ///     Log warning. Will produce warning to build output.
        /// </summary>
        void Warn(string message);

        #endregion
    }
}