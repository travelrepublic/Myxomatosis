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

        #endregion

        public int PrefetchCount { get; set; }

        #region IQueueSubscriptionManager Members

        public QueueSubscription GetSubscription(string exchange, string queueName, string routingKey, ExchangeType exchangeType)
        {
            return _subscriptions.GetOrAdd(Concatenate(exchange, queueName, routingKey),
                new QueueSubscription(exchange, queueName, routingKey, exchangeType)
                {
                    PrefetchCount = PrefetchCount
                });
        }

        #endregion

        private string Concatenate(params string[] keys)
        {
            return string.Join("::", keys.Select(k => k ?? string.Empty));
        }
    }
}