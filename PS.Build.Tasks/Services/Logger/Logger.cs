using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ILogger = PS.Build.Services.ILogger;

namespace PS.Build.Tasks.Services
{
    class Logger : MarshalByRefObject,
                   ILogger
    {
        #region Constants

        private const string IndentHolder = "  ";

        #endregion

        private readonly TaskLoggingHelper _log;

        string _indent;

        #region Constructors

        public Logger(TaskLoggingHelper log)
        {
            _log = log;
            _indent = string.Empty;
        }

        #endregion

        #region Properties

        public bool HasErrors => _log.HasLoggedErrors;

        #endregion

        #region ILogger Members

        public void Critical(string message)
        {
            _log.LogCriticalMessage(null, null, null, null, 0, 0, 0, 0, message ?? string.Empty);
        }

        public void Debug(string message)
        {
#if DEBUG
            _log.LogMessage(MessageImportance.Low, _indent + "^ " + message);
#else
            _log.LogMessage(MessageImportance.Low, _indent + message);
#endif

        }

        public void Error(string message)
        {
            _log.LogError(message ?? string.Empty);
        }

        public IDisposable IndentMessages()
        {
            _indent += IndentHolder;
            return new DelegateDisposable(() => { _indent = _indent.Substring(0, Math.Max(_indent.Length - IndentHolder.Length, 0)); });
        }

        public void Info(string message)
        {
            _log.LogMessage(MessageImportance.Normal, _indent + message);
        }

        public void Warn(string message)
        {
            _log.LogWarning(message ?? string.Empty);
        }

        #endregion
    }
}