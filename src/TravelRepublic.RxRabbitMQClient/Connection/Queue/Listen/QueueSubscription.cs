using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen
{
    public class QueueSubscription : IDisposable
    {
        private readonly ReplaySubject<RabbitMessage> _subject;

        #region Constructors

        public QueueSubscription(string exchange, string queue)
        {
            _subject = new ReplaySubject<RabbitMessage>();

            QueueName = new QueueSubscriptionData(exchange, queue);
            KeepListening = true;
            OpenEvent = new ManualResetEvent(false);
        }

        #endregion Constructors

        public QueueSubscriptionData QueueName { get; private set; }

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