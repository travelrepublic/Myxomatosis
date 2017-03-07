using System.Collections.Generic;
using Myxomatosis.Connection.Message;
using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Myxomatosis.Connection.Queue.Listen
{
    /// <summary>
    /// Centralised place to ensure that we only create one connection for each queue per AppDomain
    /// </summary>
    internal class QueueSubscriptionCache
    {
        private readonly ConcurrentDictionary<string, QueueSubscriptionToken> _subscriptions;

        #region Constructors

        public QueueSubscriptionCache()
        {
            _subscriptions = new ConcurrentDictionary<string, QueueSubscriptionToken>();
        }

        #endregion Constructors

        public QueueSubscriptionToken Create(string queueName, ushort prefetchCount, Dictionary<string, object> args )
        {
            return _subscriptions.GetOrAdd(queueName, q => new QueueSubscriptionToken(q, prefetchCount, args));
        }

        public void Remove(string queueName)
        {
            QueueSubscriptionToken removedItem;
            _subscriptions.TryRemove(queueName, out removedItem);
        }
    }

    internal class QueueSubscriptionToken : IDisposable
    {
        private int _keepListeningInt;
        private readonly ReplaySubject<RabbitMessage> _subject;

        #region Constructors

        internal QueueSubscriptionToken(string queue, ushort prefetchCount, Dictionary<string, object> args )
        {
            _subject = new ReplaySubject<RabbitMessage>();
            _keepListeningInt = 0;
            QueueName = queue;
            PrefetchCount = prefetchCount;
            Args = args;
            OpenEvent = new ManualResetEvent(false);
            ClosedEvent = new ManualResetEvent(false);
        }

        #endregion Constructors

        public string QueueName { get; private set; }
        public ushort PrefetchCount { get; private set; }
        public Dictionary<string,object> Args { get; private set; }

        public IObservable<RabbitMessage> MessageSource
        {
            get { return _subject.AsObservable(); }
        }

        public IObserver<RabbitMessage> MessageObserver
        {
            get { return _subject.AsObserver(); }
        }

        public bool KeepListening
        {
            set
            {
                var keepListeningInt = value ? 1 : 0;
                Interlocked.Exchange(ref _keepListeningInt, keepListeningInt);
            }
            get
            {
                return Interlocked.Equals(_keepListeningInt, 1);
            }
        }

        internal ManualResetEvent OpenEvent { get; private set; }
        internal ManualResetEvent ClosedEvent { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                _subject.Dispose();
                (OpenEvent as IDisposable).Dispose();
                (ClosedEvent as IDisposable).Dispose();
            }
            catch (Exception)
            {
            }
        }

        #endregion IDisposable Members
    }
}