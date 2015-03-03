namespace TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen
{
    public interface IQueueSubscriptionManager
    {
        QueueSubscription GetSubscription(string exchange, string queueName);
    }
}