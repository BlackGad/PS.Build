using System;
using System.Collections.Generic;
using PS.Build.Tasks.Extensions;

namespace PS.Build.Tasks
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services;

        #region Constructors

        public ServiceProvider()
        {
            _services = new Dictionary<Type, object>();
        }

        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            if (_services.ContainsKey(serviceType)) return _services[serviceType];
            throw new NotSupportedException();
        }

        #endregion

        #region Members

        public void AddService(Type type, object instance)
        {
            _services.Set(type, () => instance);
        }

        #endregion
    }
}