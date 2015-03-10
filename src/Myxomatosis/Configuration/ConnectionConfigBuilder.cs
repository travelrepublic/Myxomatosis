using Myxomatosis.Logging;
using Myxomatosis.Serialization;

namespace Myxomatosis.Configuration
{
    internal class ConnectionConfigBuilder : IConnectionConfigBuilder
    {
        private readonly DefaultConfiguration _configuration;

        #region Constructors

        public ConnectionConfigBuilder()
        {
            _configuration = new DefaultConfiguration();
        }

        #endregion Constructors

        #region IConnectionConfigBuilder Members

        public IConnectionConfigBuilder WithUserName(string userName)
        {
            _configuration.UserName = userName;
            return this;
        }

        public IConnectionConfigBuilder WithPassword(string password)
        {
            _configuration.Password = password;
            return this;
        }

        public IConnectionConfigBuilder WithHostName(string hostName)
        {
            _configuration.HostName = hostName;
            return this;
        }

        public IConnectionConfigBuilder WithVirtualHost(string virtualHost)
        {
            _configuration.VirtualHost = virtualHost;
            return this;
        }

        public IConnectionConfigBuilder UsingSerializer(ISerializer serializer)
        {
            _configuration.Serializer = serializer;
            return this;
        }

        public IConnectionConfigBuilder WithLogger(IRabbitMqClientLogger logger)
        {
            _configuration.Logger = logger;
            return this;
        }

        public IConnectionConfigBuilder WithPrefetchCount(int prefetchCount)
        {
            _configuration.PrefetchCount = prefetchCount;
            return this;
        }

        #endregion IConnectionConfigBuilder Members

        public IConnectionConfig Create()
        {
            return _configuration;
        }
    }
}