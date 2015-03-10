using System.Collections.Concurrent;
using System.Linq;

namespace Myxomatosis.Connection.Queue.Listen
{
    public class SubscriptionManager : IQueueSubscriptionManager
    {
        private readonly ConcurrentDictionary<string, QueueSubscription> _subscriptions;

        #region Constructors

        public SubscriptionManager()
        {
            _subscriptions = new ConcurrentDictionary<string, QueueSubscription>();
        }

        public int PrefetchCount { get; set; }

        #endregion Constructors

        #region IQueueSubscriptionManager Members

        public QueueSubscription GetSubscription(string exchange, string queueName, string routingKey)
        {
            return _subscriptions.GetOrAdd(Concatenate(exchange, queueName, routingKey),
                new QueueSubscription(exchange, queueName, routingKey)
                {
                    PrefetchCount = PrefetchCount
                });
        }

        #endregion IQueueSubscriptionManager Members

        private string Concatenate(params string[] keys)
        {
            return string.Join("::", keys.Select(k => k ?? string.Empty));
        }
    }
}