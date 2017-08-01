using System;
using PS.Build.Services;

namespace PS.Build.Tasks.Services
{
    class ScopeLogger : MarshalByRefObject,
                        ILogger
    {
        #region Constants

        private const string IndentHolder = "  ";

        #endregion

        private readonly ILogger _log;
        string _indent;

        #region Constructors

        public ScopeLogger(ILogger log)
        {
            _log = log;
            _indent = string.Empty;
            HasErrors = false;
        }

        #endregion

        #region Properties

        public bool HasErrors { get; private set; }

        #endregion

        #region ILogger Members

        public void Critical(string message)
        {
            HasErrors = true;
            _log.Critical(message);
        }

        public void Debug(string message)
        {
            _log.Debug(_indent + message);
        }

        public void Error(string message)
        {
            HasErrors = true;
            _log.Error(message);
        }

        public IDisposable IndentMessages()
        {
            _indent += IndentHolder;
            return new DelegateDisposable(() => { _indent = _indent.Substring(0, Math.Max(_indent.Length - IndentHolder.Length, 0)); });
        }

        public void Info(string message)
        {
            _log.Info(_indent + message);
        }

        public void Warn(string message)
        {
            _log.Warn(message);
        }

        #endregion
    }
}