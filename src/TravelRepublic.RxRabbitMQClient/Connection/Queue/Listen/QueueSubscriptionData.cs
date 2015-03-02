namespace TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen
{
    public class QueueSubscriptionData
    {
        public QueueSubscriptionData(string exchange, string queue)
        {
            Exchange = exchange;
            Queue = queue;
            Type = "fanout";
        }

        public string Exchange { get; private set; }

        public string Queue { get; private set; }

        public string Type { get; private set; }
    }
}