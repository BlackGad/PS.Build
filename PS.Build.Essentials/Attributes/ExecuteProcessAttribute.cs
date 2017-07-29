using System;
using System.ComponentModel;
using System.Diagnostics;
using PS.Build.Essentials.Extensions;
using PS.Build.Extensions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Essentials.Attributes
{
    /// <summary>
    ///     Starts the process resource that is specified by the parameter containing process start information (for example,
    ///     the file name of the process to start) and associates the resource with a new Process component.
    ///     <see cref="https://msdn.microsoft.com/en-us/library/0w4h05yb(v=vs.110).aspx" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public class ExecuteProcessAttribute : Attribute
    {
        #region Constructors

        public ExecuteProcessAttribute(string filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");
            Filename = filename;

            CreateNoWindow = true;
            WindowStyle = ProcessWindowStyle.Hidden;
            WaitForProcessExit = true;
            BuildStep = BuildStep.PostBuild;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the set of command-line arguments to use when starting the application.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to start the process.
        /// </summary>
        public BuildStep BuildStep { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to start the process in a new window.
        /// </summary>
        public bool CreateNoWindow { get; set; }

        /// <summary>
        ///     Gets or sets a value that identifies the domain to use when starting the process.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        ///     Gets the environment variables that apply to this process and its child processes. Semicolon separated pairs of key
        ///     and value.
        /// </summary>
        /// <example>
        ///     Variable1=Value1;Variable2=42
        /// </example>
        public string Environment { get; set; }

        /// <summary>
        ///     Gets the application or document to start.
        /// </summary>
        public string Filename { get; protected set; }

        /// <summary>
        ///     Gets or sets a secure string that contains the user password to use when starting the process.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the user name to be used when starting the process.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use the operating system shell to start the process.
        /// </summary>
        public bool UseShellExecute { get; set; }

        /// <summary>
        ///     Gets or sets option to wait process exit
        /// </summary>
        public bool WaitForProcessExit { get; set; }

        /// <summary>
        ///     Gets or sets the window state to use when the process is started.
        /// </summary>
        public ProcessWindowStyle WindowStyle { get; set; }

        /// <summary>
        ///     When the UseShellExecute property is false, gets or sets the working directory for the process to be started. When
        ///     UseShellExecute is true, gets or sets the directory that contains the process to be started.
        /// </summary>
        public string WorkingDirectory { get; set; }

        #endregion

        #region Members

        protected virtual void StartProcess(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var macroResolver = provider.GetService<IMacroResolver>();
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    Arguments = Arguments,
                    CreateNoWindow = CreateNoWindow,
                    Domain = Domain,
                    FileName = macroResolver.Resolve(Filename),
                    Password = Password.ToSecureString(),
                    UserName = Username,
                    UseShellExecute = UseShellExecute,
                    WindowStyle = WindowStyle,
                    WorkingDirectory = macroResolver.Resolve(WorkingDirectory),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                foreach (var pair in (Environment ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var keyValue = pair.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length != 2)
                    {
                        logger.Error("Environment variable: " + keyValue + " invalid");
                        return;
                    }
                    startInfo.EnvironmentVariables.Add(macroResolver.Resolve(keyValue[0]),
                                                       macroResolver.Resolve(keyValue[1]));
                }

                var process = new Process
                {
                    StartInfo = startInfo
                };

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data)) logger.Info(args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data)) logger.Error(args.Data);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (WaitForProcessExit) process.WaitForExit();
            }
            catch (Exception e)
            {
                logger.Error("Process execution failed. Details: " + e.GetBaseException().Message);
            }
        }

        private void PostBuild(IServiceProvider provider)
        {
            if (BuildStep.HasFlag(BuildStep.PostBuild)) StartProcess(provider);
        }

        private void PreBuild(IServiceProvider provider)
        {
            if (BuildStep.HasFlag(BuildStep.PreBuild)) StartProcess(provider);
        }

        #endregion
    }
}