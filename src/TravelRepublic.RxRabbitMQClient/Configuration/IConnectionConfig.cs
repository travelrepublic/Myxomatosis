using TravelRepublic.RxRabbitMQClient.Logging;
using TravelRepublic.RxRabbitMQClient.Serialization;

namespace TravelRepublic.RxRabbitMQClient.Configuration
{
    public interface IConnectionConfig
    {
        string VirtualHost { get; }

        string UserName { get; }

        string Password { get; }

        string HostName { get; }

        ISerializer Serializer { get; }

        IRabbitMqClientLogger Logger { get; }
    }
}