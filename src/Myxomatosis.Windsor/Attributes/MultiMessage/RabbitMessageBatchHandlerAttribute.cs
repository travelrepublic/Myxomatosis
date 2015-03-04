using System;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RabbitMessageBatchHandlerAttribute : RabbitMessageHandlerAttribute, IStaticBatchSubscriptionConfig, IConfigFactory<IBatchSubscriptionConfig>
    {
        #region Constructors

        public RabbitMessageBatchHandlerAttribute(string exchange, string subscriberQueue)
            : base(exchange, subscriberQueue)
        {
        }

        #endregion

        #region IConfigFactory<IBatchSubscriptionConfig> Members

        public new IBatchSubscriptionConfig GetConfig()
        {
            return new BatchConverter().Convert(this);
        }

        #endregion

        #region IStaticBatchSubscriptionConfig Members

        public double BufferTimeout { get; set; }
        public int BufferSize { get; set; }

        #endregion
    }
}