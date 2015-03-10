using Myxomatosis.Connection.Message;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Myxomatosis.Connection.Queue.Listen
{
    public class QueueSubscription : IDisposable
    {
        private readonly ReplaySubject<RabbitMessage> _subject;

        #region Constructors

        public QueueSubscription(string exchange, string queue, string routingKey, ExchangeType exchangeType)
        {
            _subject = new ReplaySubject<RabbitMessage>();

            SubscriptionData = new QueueSubscriptionData(exchange, queue, routingKey)
            {
                Type = exchangeType
            };
            KeepListening = true;
            OpenEvent = new ManualResetEvent(false);
        }

        #endregion Constructors

        public int PrefetchCount
        {
            set { SubscriptionData.PrefetchCount = value; }
        }

        public QueueSubscriptionData SubscriptionData { get; private set; }

        public IObservable<RabbitMessage> MessageSource
        {
            get { return _subject.AsObservable(); }
        }

        public IObserver<RabbitMessage> MessageObserver
        {
            get { return _subject; }
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