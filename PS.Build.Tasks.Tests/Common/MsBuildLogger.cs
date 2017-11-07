using System;
using System.Text;
using Microsoft.Build.Framework;

namespace PS.Build.Tasks.Tests.Common
{
    public class MsBuildLogger : ILogger
    {
        StringBuilder _builder;
        private int _indent;

        #region Properties

        /// <summary>
        ///     This property holds the user-specified parameters to the logger. If parameters are not provided, a logger should
        ///     revert
        ///     to defaults. If a logger does not take parameters, it can ignore this property.
        /// </summary>
        /// <value>
        ///     The parameter string (can be null).
        /// </value>
        public string Parameters { get; set; }

        /// <summary>
        ///     The verbosity level directs the amount of detail that appears in a logger's event log. Though this is only a
        ///     recommendation based on user preferences, and a logger is free to choose the exact events it logs, it is still
        ///     important that the guidelines for each level be followed, for a good user experience.
        /// </summary>
        /// <value>
        ///     The verbosity level.
        /// </value>
        public LoggerVerbosity Verbosity { get; set; }

        #endregion

        #region ILogger Members

        public void Initialize(IEventSource eventSource)
        {
            if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));

            eventSource.ProjectStarted += eventSource_ProjectStarted;
            eventSource.TaskStarted += eventSource_TaskStarted;
            eventSource.MessageRaised += eventSource_MessageRaised;
            eventSource.WarningRaised += eventSource_WarningRaised;
            eventSource.ErrorRaised += eventSource_ErrorRaised;
            eventSource.ProjectFinished += eventSource_ProjectFinished;
            _builder = new StringBuilder();
        }

        /// <summary>
        ///     Called by the build engine to allow loggers to release any resources they may have allocated at initialization
        ///     time,
        ///     or during the build.
        /// </summary>
        public void Shutdown()
        {
        }

        #endregion

        #region Event handlers

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            // BuildErrorEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = $": ERROR {e.File}({e.LineNumber},{e.ColumnNumber}): ";
            WriteLineWithSenderAndMessage(line, e);
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            // BuildMessageEventArgs adds Importance to BuildEventArgs
            // Let's take account of the verbosity setting we've been passed in deciding whether to log the message
            if ((e.Importance == MessageImportance.High && Verbosity > LoggerVerbosity.Minimal)
                || (e.Importance == MessageImportance.Normal && Verbosity > LoggerVerbosity.Normal)
                || (e.Importance == MessageImportance.Low && Verbosity > LoggerVerbosity.Detailed))
            {
                WriteLineWithSenderAndMessage(string.Empty, e);
            }
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            // The regular message string is good enough here too.
            _indent--;
            WriteLine(string.Empty, e);
        }

        void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            // ProjectStartedEventArgs adds ProjectFile, TargetNames
            // Just the regular message string is good enough here, so just display that.
            WriteLine(string.Empty, e);
            _indent++;
        }

        void eventSource_TaskStarted(object sender, TaskStartedEventArgs e)
        {
            // TaskStartedEventArgs adds ProjectFile, TaskFile, TaskName
            // To keep this log clean, this logger will ignore these events.
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            // BuildWarningEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = $": Warning {e.File}({e.LineNumber},{e.ColumnNumber}): ";
            WriteLineWithSenderAndMessage(line, e);
        }

        #endregion

        #region Members

        public string GetLog()
        {
            return _builder.ToString();
        }

        private void WriteLine(string line, BuildEventArgs e)
        {
            for (int i = _indent; i > 0; i--)
            {
                _builder.Append("    ");
            }
            _builder.AppendLine(line + e.Message);
        }

        private void WriteLineWithSenderAndMessage(string line, BuildEventArgs e)
        {
            if (string.Equals(e.SenderName, "MSBuild", StringComparison.OrdinalIgnoreCase))
                WriteLine(line, e);
            else
                WriteLine(e.SenderName + ": " + line, e);
        }

        #endregion
    }
}