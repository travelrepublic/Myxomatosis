using TravelRepublic.RxRabbitMQClient.Connection;
using TravelRepublic.RxRabbitMQClient.Connection.Exchange;
using TravelRepublic.RxRabbitMQClient.Connection.Queue;
using TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen;
using TravelRepublic.RxRabbitMQClient.Serialization;

namespace TravelRepublic.RxRabbitMQClient.Api
{
    public class Listener : IObservableConnection
    {
        private readonly IRabbitPublisher _publisher;
        private readonly ISerializer _serializer;
        private readonly IRabbitMqSubscriber _subscriberThread;
        private readonly IQueueSubscriptionManager _subscriptionManager;

        #region Constructors

        public Listener(
            IRabbitMqSubscriber subscriberThread,
            IRabbitPublisher publisher,
            IQueueSubscriptionManager subscriptionManager,
            ISerializer serializer)
        {
            _subscriberThread = subscriberThread;
            _publisher = publisher;
            _subscriptionManager = subscriptionManager;
            _serializer = serializer;
        }

        #endregion Constructors

        public IRabbitQueue GetQueue(string exchange, string queueName)
        {
            return OpenConnectionInternal(exchange, queueName);
        }

        public IRabbitQueue<T> GetQueue<T>(string exchange, string queueName)
        {
            return new QueueResult<T>(OpenConnectionInternal(exchange, queueName), _serializer);
        }

        private QueueResult OpenConnectionInternal(string exchange, string queueName)
        {
            return new QueueResult(exchange, queueName, _subscriptionManager, _publisher, _subscriberThread);
        }
    }
}