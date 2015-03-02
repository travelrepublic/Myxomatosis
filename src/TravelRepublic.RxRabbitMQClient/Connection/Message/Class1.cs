using System;

namespace TravelRepublic.RxRabbitMQClient.Connection.Message
{
    public interface IRabbitMessageHandler<T>
    {
        void Handle(T message);
    }

    public class DelegateHandler<T> : IRabbitMessageHandler<T>
    {
        private readonly Action<T> _handler;

        #region Constructors

        public DelegateHandler(Action<T> handler)
        {
            _handler = handler;
        }

        #endregion Constructors

        #region IRabbitMessageHandler<T> Members

        public void Handle(T message)
        {
            _handler(message);
        }

        #endregion IRabbitMessageHandler<T> Members

        public static implicit operator DelegateHandler<T>(Action<T> handler)
        {
            return new DelegateHandler<T>(handler);
        }
    }

    public interface ISubscriptionConfig
    {
        string Id { get; set; }

        string Name { get; set; }

        TimeSpan Interval { get; }

        string QueueName { get; }

        string SubscriptionId { get; set; }

        TimeSpan OpenTimeout { get; }

        TimeSpan CloseTimeout { get; }

        string Exchange { get; }
    }

    public class DefaultSubscriptionConfig : ISubscriptionConfig
    {
        #region Constructors

        public DefaultSubscriptionConfig(string exchange, string queueName)
        {
            Interval = TimeSpan.FromSeconds(0);
            OpenTimeout = TimeSpan.FromMinutes(1);
            CloseTimeout = TimeSpan.FromMinutes(1);
            SubscriptionId = string.Empty;
            Id = Guid.NewGuid().ToString();

            Exchange = exchange;
            QueueName = queueName;
            Name = string.Format("{0}::{1}", Exchange, QueueName);
        }

        #endregion Constructors

        #region ISubscriptionConfig Members

        public string Id { get; set; }

        public string Name { get; set; }

        public TimeSpan Interval { get; set; }

        public string QueueName { get; private set; }

        public string SubscriptionId { get; set; }

        public TimeSpan OpenTimeout { get; set; }

        public TimeSpan CloseTimeout { get; set; }

        public string Exchange { get; private set; }

        #endregion ISubscriptionConfig Members
    }

    public interface IBatchSubscriptionConfig : ISubscriptionConfig
    {
        TimeSpan BufferTimeout { get; }

        int BufferSize { get; }
    }

    public class DefaultBatchConfig : DefaultSubscriptionConfig, IBatchSubscriptionConfig
    {
        #region Constructors

        public DefaultBatchConfig(string exchange, string subscriberQueue)
            : base(exchange, subscriberQueue)
        {
            BufferSize = 3;
            BufferTimeout = TimeSpan.FromSeconds(10);
        }

        #endregion Constructors

        #region IBatchSubscriptionConfig Members

        public TimeSpan BufferTimeout { get; set; }

        public int BufferSize { get; set; }

        #endregion IBatchSubscriptionConfig Members
    }
}