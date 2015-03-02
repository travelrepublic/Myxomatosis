namespace TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen
{
    public interface IRabbitMqSubscriber
    {
        void Subscribe(QueueSubscription subscription);
    }
}