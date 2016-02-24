using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MediaComplete
{
    /// <summary>
    /// Observable boolean for property bindings.
    /// </summary>
    public class ObservableBool : INotifyPropertyChanged, IObservable<bool>
    {
        /// <summary>
        /// Contains the value of the boolean.
        /// </summary>
        public bool Value
        {
            get { return _value; }
            set
            {
                _value = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                _subscribers.ForEach(s => s.OnNext(_value));
            }
        }

        private bool _value;
        private readonly List<Subscriber> _subscribers = new List<Subscriber>();

        /// <summary>
        /// Subscribe to the observable.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<bool> observer)
        {
            var sub = new Subscriber(observer, this);
            _subscribers.Add(sub);
            return sub;
        }

        /// <summary>
        /// Private class containing internal references to subscribers.
        /// </summary>
        private class Subscriber : IDisposable
        {
            private readonly IObserver<bool> _observer;
            private readonly ObservableBool _parent;

            public Subscriber(IObserver<bool> observer, ObservableBool parent)
            {
                _observer = observer;
                _parent = parent;
            }

            public void OnNext(bool value)
            {
                _observer.OnNext(value);
            }

            public void Dispose()
            {
                _parent._subscribers.Remove(this);
            }
        }

        /// <summary>
        /// Notifies the UI element that the boolean value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate {};
    }
}
