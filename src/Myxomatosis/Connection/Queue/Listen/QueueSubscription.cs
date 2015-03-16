using Myxomatosis.Connection.Message;
using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Myxomatosis.Connection.Queue.Listen
{
    public class QueueSubscriptionCache
    {
        private readonly ConcurrentDictionary<string, QueueSubscription> _subscriptions;

        #region Constructors

        public QueueSubscriptionCache()
        {
            _subscriptions = new ConcurrentDictionary<string, QueueSubscription>();
        }

        #endregion Constructors

        public QueueSubscription Create(string queueName, ushort prefetchCount)
        {
            return _subscriptions.GetOrAdd(queueName, q => new QueueSubscription(q, prefetchCount));
        }

        public void Remove(string queueName)
        {
            QueueSubscription removedItem;
            _subscriptions.TryRemove(queueName, out removedItem);
        }
    }

    public class QueueSubscription : IDisposable
    {
        private readonly ReplaySubject<RabbitMessage> _subject;

        #region Constructors

        internal QueueSubscription(string queue, ushort prefetchCount)
        {
            _subject = new ReplaySubject<RabbitMessage>();
            SubscriptionData = new QueueSubscriptionData(queue)
            {
                PrefetchCount = prefetchCount
            };
            KeepListening = true;
            OpenEvent = new ManualResetEvent(false);
        }

        #endregion Constructors

        public QueueSubscriptionData SubscriptionData { get; private set; }

        public IObservable<RabbitMessage> MessageSource
        {
            get { return _subject.AsObservable(); }
        }

        public IObserver<RabbitMessage> MessageObserver
        {
            get { return _subject.AsObserver(); }
        }

        public Task ConsumingTask { get; internal set; }

        public bool KeepListening { get; internal set; }

        public ManualResetEvent OpenEvent { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                _subject.Dispose();
            }
            catch (Exception)
            {
            }
        }

        #endregion IDisposable Members
    }
}