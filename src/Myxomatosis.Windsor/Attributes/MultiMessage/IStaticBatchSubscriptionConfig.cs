namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
{
    public interface IStaticBatchSubscriptionConfig : IStaticSubscriptionConfig
    {
        double BufferTimeout { get; set; }

        int BufferSize { get; set; }
    }
}