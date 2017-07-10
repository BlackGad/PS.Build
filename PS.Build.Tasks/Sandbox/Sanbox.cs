using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PS.Build.Services;

namespace PS.Build.Tasks
{
    public class Sanbox : IDisposable
    {
        private readonly AppDomain _appDomain;

        #region Constructors

        public Sanbox(IExplorer explorer)
        {
            var domainSetup = new AppDomainSetup
            {
                //ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location),
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                ConfigurationFile = Assembly.GetExecutingAssembly().Location + ".config"
            };

            try
            {
                _appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString("N"), AppDomain.CurrentDomain.Evidence, domainSetup);
                AssemblyResolver = Create<SandboxAssemblyResolver>(new object[] { explorer.References.Select(r => r.FullPath).ToArray() });
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

        public SandboxAssemblyResolver AssemblyResolver { get; }

        public SandboxClient Client { get; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            AssemblyResolver.Dispose();
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