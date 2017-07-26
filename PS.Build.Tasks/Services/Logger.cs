using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ILogger = PS.Build.Services.ILogger;

namespace PS.Build.Tasks.Services
{
    internal class Logger : MarshalByRefObject,
                            ILogger
    {
        private readonly TaskLoggingHelper _log;

        #region Constructors

        public Logger(TaskLoggingHelper log)
        {
            _log = log;
        }

        #endregion

        #region ILogger Members

        public void Critical(string message)
        {
            _log.LogCriticalMessage(null, null, null, null, 0, 0, 0, 0, message ?? string.Empty);
        }

        public void Debug(string message)
        {
            _log.LogMessage(MessageImportance.Low, message ?? string.Empty);
        }

        public void Error(string message)
        {
            _log.LogError(message ?? string.Empty);
        }

        public void Info(string message)
        {
            _log.LogMessage(MessageImportance.Normal, message ?? string.Empty);
        }

        public void Warn(string message)
        {
            _log.LogWarning(message ?? string.Empty);
        }

        #endregion
    }
}