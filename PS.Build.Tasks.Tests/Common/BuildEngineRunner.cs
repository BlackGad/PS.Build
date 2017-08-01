using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using NUnit.Framework;
using PS.Build.Extensions;

namespace PS.Build.Tasks.Tests.Common
{
    public class BuildEngineRunner : IDisposable
    {
        private readonly string _projectPath;
        private readonly Dictionary<object, object> _taskObjectTable;

        readonly Dictionary<Task, TaskEvents> _taskResults;

        #region Constructors

        public BuildEngineRunner(string projectPath)
        {
            if (projectPath == null) throw new ArgumentNullException(nameof(projectPath));
            Assert.IsTrue(File.Exists(projectPath));

            _projectPath = projectPath;
            _taskObjectTable = new Dictionary<object, object>();
            _taskResults = new Dictionary<Task, TaskEvents>();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            foreach (var instance in _taskObjectTable.Values)
            {
                var disposable = instance as IDisposable;
                disposable?.Dispose();
            }
        }

        #endregion

        #region Members

        public T Create<T>() where T : Task, new()
        {
            var taskResult = new TaskEvents();
            var engineMock = new Mock<IBuildEngine4>();

            engineMock.Setup(engine => engine.LogMessageEvent(It.IsAny<BuildMessageEventArgs>()))
                      .Callback<BuildMessageEventArgs>(ev =>
                      {
                          if (Debugger.IsAttached) Debug.WriteLine(ev.Message);
                          Console.WriteLine(ev.Message);
                          lock (taskResult) taskResult.Messages.Add(ev);
                      });

            engineMock.Setup(engine => engine.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
                      .Callback<BuildErrorEventArgs>(ev =>
                      {
                          if (Debugger.IsAttached) Debug.WriteLine("E: " + ev.Message);
                          Console.WriteLine("E: " + ev.Message);
                          lock (taskResult) taskResult.Errors.Add(ev);
                      });

            engineMock.Setup(engine => engine.LogWarningEvent(It.IsAny<BuildWarningEventArgs>()))
                      .Callback<BuildWarningEventArgs>(ev =>
                      {
                          if (Debugger.IsAttached) Debug.WriteLine("W: " + ev.Message);
                          Console.WriteLine("W: " + ev.Message);
                          lock (taskResult) taskResult.Warnings.Add(ev);
                      });

            engineMock.Setup(engine => engine.LogCustomEvent(It.IsAny<CustomBuildEventArgs>()))
                      .Callback<CustomBuildEventArgs>(ev =>
                      {
                          if (Debugger.IsAttached) Debug.WriteLine("C: " + ev.Message);
                          Console.WriteLine("C: " + ev.Message);
                          lock (taskResult) taskResult.Custom.Add(ev);
                      });

            engineMock.Setup(engine => engine.RegisterTaskObject(It.IsAny<object>(),
                                                                 It.IsAny<object>(),
                                                                 It.IsAny<RegisteredTaskObjectLifetime>(),
                                                                 It.IsAny<bool>()))
                      .Callback(
                          (object key, object obj, RegisteredTaskObjectLifetime lt, bool allowEarlyCollection) =>
                          {
                              _taskObjectTable.Set(key, () => obj);
                          });

            engineMock.Setup(engine => engine.GetRegisteredTaskObject(It.IsAny<object>(),
                                                                      It.IsAny<RegisteredTaskObjectLifetime>()))
                      .Returns<object, RegisteredTaskObjectLifetime>((key, lifetime) => _taskObjectTable.Get(key));

            engineMock.Setup(engine => engine.ProjectFileOfTaskNode)
                      .Returns(() => _projectPath);

            var task = new T
            {
                BuildEngine = engineMock.Object,
            };

            _taskResults.Set(task, () => taskResult);
            return task;
        }

        public TaskEvents GetEvents(Task task)
        {
            return _taskResults.Get(task);
        }

        #endregion
    }
}