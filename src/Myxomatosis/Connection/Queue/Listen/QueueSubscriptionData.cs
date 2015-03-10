namespace Myxomatosis.Connection.Queue.Listen
{
    public class QueueSubscriptionData
    {
        #region Constructors

        public QueueSubscriptionData(string exchange, string queue, string routingKey)
        {
            Exchange = exchange;
            Queue = queue;
            RoutingKey = routingKey;
            Type = ExchangeType.Topic;
            PrefetchCount = 50;
        }

        #endregion Constructors

        public string Exchange { get; private set; }

        public string Queue { get; private set; }

        public string RoutingKey { get; set; }

        public ExchangeType Type { get; set; }

        public int PrefetchCount { get; set; }
    }
}