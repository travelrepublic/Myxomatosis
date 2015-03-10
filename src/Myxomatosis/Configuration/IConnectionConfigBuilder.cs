using Myxomatosis.Logging;
using Myxomatosis.Serialization;

namespace Myxomatosis.Configuration
{
    public interface IConnectionConfigBuilder
    {
        IConnectionConfigBuilder WithUserName(string userName);

        IConnectionConfigBuilder WithPassword(string password);

        IConnectionConfigBuilder WithHostName(string hostName);

        IConnectionConfigBuilder WithVirtualHost(string virtualHost);

        IConnectionConfigBuilder UsingSerializer(ISerializer serializer);

        IConnectionConfigBuilder WithLogger(IRabbitMqClientLogger logger);

        IConnectionConfigBuilder WithPrefetchCount(int prefetchCount);
    }
}