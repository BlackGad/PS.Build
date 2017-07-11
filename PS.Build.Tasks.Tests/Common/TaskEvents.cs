using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace PS.Build.Tasks.Tests.Common
{
    public class TaskEvents
    {
        #region Constructors

        public TaskEvents()
        {
            Messages = new List<BuildMessageEventArgs>();
            Warnings = new List<BuildWarningEventArgs>();
            Errors = new List<BuildErrorEventArgs>();
            Custom = new List<CustomBuildEventArgs>();
        }

        #endregion

        #region Properties

        public List<CustomBuildEventArgs> Custom { get; }
        public List<BuildErrorEventArgs> Errors { get; }
        public List<BuildMessageEventArgs> Messages { get; }
        public List<BuildWarningEventArgs> Warnings { get; }

        #endregion
    }
}