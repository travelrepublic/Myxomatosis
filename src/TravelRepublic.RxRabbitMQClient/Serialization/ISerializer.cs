namespace TravelRepublic.RxRabbitMQClient.Serialization
{
    public interface ISerializer
    {
        byte[] Serialize(object payload);

        T Deserialize<T>(byte[] bytes);
    }
}