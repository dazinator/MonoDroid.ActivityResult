using System;
using System.Collections.Generic;
using System.Threading;

namespace MonoDroid.ActivityResult
{
    public class ConcurrentSubscriberList<T>
    {
        private readonly List<T> _listeners;
        private ReaderWriterLockSlim _activityResultlistenersLock = new ReaderWriterLockSlim();

        public void Add(T subscriber)
        {
            using (_activityResultlistenersLock.Write())
            {
                _listeners.Add(subscriber);
                // do writing here
            }
        }

        public void Remove(T subscriber)
        {
            using (_activityResultlistenersLock.Write())
            {
                _listeners.Remove(subscriber);
                // do writing here
            }
        }

        public void NotifyAll(Action<T> notifyAction)
        {
            using (_activityResultlistenersLock.Read())
            {
                foreach (var item in _listeners)
                {
                    notifyAction(item);
                }
            }
        }
    }
}

