using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using PS.Build.Tasks.Extensions;

namespace PS.Build.Tasks.Tests.Common
{
    public static class BuildEngineRunner
    {
        #region Static members

        public static T Create<T>(string projectFilePath,
                                  Dictionary<object, object> taskObjectTable,
                                  Action<BuildMessageEventArgs> logMessage,
                                  Action<BuildErrorEventArgs> logError) where T : Task, new()
        {
            var engineMock = new Mock<IBuildEngine4>();
            engineMock.Setup(engine => engine.LogMessageEvent(It.IsAny<BuildMessageEventArgs>()))
                      .Callback<BuildMessageEventArgs>(ev =>
                      {
                          if (Debugger.IsAttached) Debug.WriteLine(ev.Message);
                          logMessage?.Invoke(ev);
                      });

            engineMock.Setup(engine => engine.RegisterTaskObject(It.IsAny<object>(),
                                                                 It.IsAny<object>(),
                                                                 It.IsAny<RegisteredTaskObjectLifetime>(),
                                                                 It.IsAny<bool>()))
                      .Callback(
                          (object key, object obj, RegisteredTaskObjectLifetime lt, bool allowEarlyCollection) =>
                          {
                              taskObjectTable.Set(key, () => obj);
                          });

            engineMock.Setup(engine => engine.GetRegisteredTaskObject(It.IsAny<object>(),
                                                                      It.IsAny<RegisteredTaskObjectLifetime>()))
                      .Returns<object, RegisteredTaskObjectLifetime>((key, lifetime) => taskObjectTable.Get(key));

            engineMock.Setup(engine => engine.ProjectFileOfTaskNode)
                      .Returns(() => projectFilePath);

            engineMock.Setup(engine => engine.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
                      .Callback<BuildErrorEventArgs>(ev =>
                      {
                          if (Debugger.IsAttached) Debug.WriteLine("E: " + ev.Message);
                          logError?.Invoke(ev);
                      });
            var task = new T
            {
                BuildEngine = engineMock.Object,
            };
            return task;
        }

        #endregion
    }
}