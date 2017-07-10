using System;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PS.Build.Tasks.Tests.Tasks
{
    public class MsBuildLogger : Logger
    {
        StringBuilder _builder;
        private int _indent;

        #region Override members

        public override void Initialize(IEventSource eventSource)
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
            if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                || (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal))
                || (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed))
                )
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