using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PS.Build.Services;

namespace PS.Build.Tasks
{
    class Sanbox : IDisposable
    {
        private readonly AppDomain _appDomain;
        private readonly ILogger _logger;

        #region Constructors

        public Sanbox(IExplorer explorer, ILogger logger)
        {
            _logger = logger;
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
            TaskAssemblyResolver = new DomainAssemblyResolver(additionalReferenceDirectories,
                                                              Enumerable.Empty<string>().ToArray(),
                                                              _logger);
            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(executingAssembly.Location),
                ConfigurationFile = configurationFile
            };

            try
            {
                _appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString("N"), AppDomain.CurrentDomain.Evidence, domainSetup);
                SandboxAssemblyResolver = Create<DomainAssemblyResolver>(additionalReferenceDirectories,
                                                                         explorer.References.Select(r => r.FullPath).ToArray(),
                                                                         _logger);
                Client = Create<SandboxClient>(explorer);
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
        public DomainAssemblyResolver TaskAssemblyResolver { get; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            TaskAssemblyResolver.Dispose();
            SandboxAssemblyResolver.Dispose();
            AppDomain.Unload(_appDomain);
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