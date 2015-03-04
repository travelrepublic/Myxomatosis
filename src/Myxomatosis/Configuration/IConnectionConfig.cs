using Myxomatosis.Logging;
using Myxomatosis.Serialization;

namespace Myxomatosis.Configuration
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