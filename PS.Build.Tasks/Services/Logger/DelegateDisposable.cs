using System;

namespace PS.Build.Tasks.Services
{
    internal class DelegateDisposable : MarshalByRefObject,
                                        IDisposable
    {
        private readonly Action _action;

        #region Constructors

        public DelegateDisposable(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _action();
        }

        #endregion
    }
}