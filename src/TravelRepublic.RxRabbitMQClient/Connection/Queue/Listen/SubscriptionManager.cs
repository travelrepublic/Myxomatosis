using System.Collections.Concurrent;

namespace TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen
{
    public class SubscriptionManager : IQueueSubscriptionManager
    {
        private readonly ConcurrentDictionary<string, QueueSubscription> _subscriptions;

        public SubscriptionManager()
        {
            _subscriptions = new ConcurrentDictionary<string, QueueSubscription>();
        }

        public QueueSubscription GetSubscription(string exchange, string queueName, string subscriptionKey)
        {
            return _subscriptions.GetOrAdd(Concatenate(exchange, queueName, subscriptionKey), new QueueSubscription(exchange, queueName));
        }

        public QueueSubscription GetSubscription(string exchange, string queueName)
        {
            return _subscriptions.GetOrAdd(Concatenate(exchange, queueName), new QueueSubscription(exchange, queueName));
        }

        private string Concatenate(params string[] keys)
        {
            return string.Join("::", keys);
        }
    }
}