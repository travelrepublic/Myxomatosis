namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
{
    public interface IConfigFactory<TConfig>
    {
        TConfig GetConfig();
    }
}