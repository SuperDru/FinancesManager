using System;
using System.Collections.Generic;

namespace FinanceManagement.Common
{
    public class UnsubscribeContext<T>: IDisposable
    {
        private readonly List<IObserver<T>>_observers;
        private readonly IObserver<T> _observer;

        public UnsubscribeContext(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}