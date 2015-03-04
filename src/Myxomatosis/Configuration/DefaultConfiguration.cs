using Myxomatosis.Logging;
using Myxomatosis.Serialization;

namespace Myxomatosis.Configuration
{
    public class DefaultConfiguration : IConnectionConfig
    {
        #region Constructors

        public DefaultConfiguration()
        {
            Serializer = DefaultSerializer.Instance;
            VirtualHost = "/";
            UserName = "guest";
            Password = "guest";
            HostName = "localhost";
            Logger = new RabbitMqConsoleLogger();
        }

        #endregion Constructors

        #region IConnectionConfig Members

        public string VirtualHost { get; internal set; }

        public string UserName { get; internal set; }

        public string Password { get; internal set; }

        public string HostName { get; internal set; }

        public ISerializer Serializer { get; internal set; }

        public IRabbitMqClientLogger Logger { get; internal set; }

        #endregion IConnectionConfig Members
    }
}