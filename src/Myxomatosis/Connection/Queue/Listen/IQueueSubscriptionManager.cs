namespace Myxomatosis.Connection.Queue.Listen
{
    public interface IQueueSubscriptionManager
    {
        QueueSubscription GetSubscription(string exchange, string queueName);
    }
}