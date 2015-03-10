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
            Type = "topic";
            PrefetchCount = 50;
        }

        #endregion Constructors

        public string Exchange { get; private set; }

        public string Queue { get; private set; }

        public string RoutingKey { get; set; }

        public string Type { get; private set; }

        public int PrefetchCount { get; set; }
    }
}