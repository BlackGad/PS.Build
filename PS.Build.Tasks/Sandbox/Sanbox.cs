using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using PS.Build.Services;

namespace PS.Build.Tasks
{
    class Sanbox : IDisposable
    {
        private readonly AppDomain _appDomain;
        private readonly object _disposeLocker;
        private readonly ILogger _logger;
        private bool _isDisposed;

        #region Constructors

        public Sanbox(IExplorer explorer, ILogger logger)
        {
            _logger = logger;
            _disposeLocker = new object();
            var executingAssembly = Assembly.GetExecutingAssembly();
            var additionalReferenceDirectories = new[]
            {
                Path.GetDirectoryName(executingAssembly.Location),
                AppDomain.CurrentDomain.BaseDirectory
            };
            var configurationFile = executingAssembly.Location + ".config";
            if (!File.Exists(configurationFile))
            {
                //Unit tests
                configurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config");
            }

            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(executingAssembly.Location),
                ConfigurationFile = configurationFile,
                
            };

            try
            {
                _appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString("N"), AppDomain.CurrentDomain.Evidence, domainSetup);
                SandboxAssemblyResolver = Create<DomainAssemblyResolver>(additionalReferenceDirectories, _logger);
                Client = Create<SandboxClient>(SandboxAssemblyResolver, explorer);
            }
            catch
            {
                AppDomain.Unload(_appDomain);
                throw;
            }
        }

        #endregion

        #region Properties

        public SandboxClient Client { get; }

        public DomainAssemblyResolver SandboxAssemblyResolver { get; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (_disposeLocker)
            {
                if (_isDisposed) return;
                _logger.Info("$ Sandbox disposed");
                SandboxAssemblyResolver.Dispose();
                AppDomain.Unload(_appDomain);

                _isDisposed = true;
            }
        }

        #endregion

        #region Members

        public T Create<T>(params object[] args) where T : MarshalByRefObject
        {
            var type = typeof(T);
            return (T)_appDomain.CreateInstanceAndUnwrap(type.Assembly.FullName,
                                                         type.FullName,
                                                         false,
                                                         BindingFlags.Default,
                                                         null,
                                                         args,
                                                         null,
                                                         null);
        }

        #endregion
    }
}