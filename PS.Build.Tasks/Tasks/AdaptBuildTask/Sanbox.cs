using System;
using System.IO;
using System.Reflection;

namespace PS.Build.Tasks
{
    public class Sanbox : IDisposable
    {
        private readonly AppDomain _appDomain;

        #region Constructors

        public Sanbox()
        {
            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location),
                ConfigurationFile = Assembly.GetExecutingAssembly().Location + ".config"
            };

            _appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString("N"), AppDomain.CurrentDomain.Evidence, domainSetup);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
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