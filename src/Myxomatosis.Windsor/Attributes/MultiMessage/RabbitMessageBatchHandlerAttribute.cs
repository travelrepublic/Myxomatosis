using Myxomatosis.Connection.Message;
using Myxomatosis.Windsor.Attributes.SingleMessage;
using System;

namespace Myxomatosis.Windsor.Attributes.MultiMessage
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RabbitMessageBatchHandlerAttribute : RabbitMessageHandlerAttribute, IStaticBatchSubscriptionConfig, IConfigFactory<IBatchSubscriptionConfig>
    {
        #region Constructors

        public RabbitMessageBatchHandlerAttribute(string exchange, string subscriberQueue)
            : base(exchange, subscriberQueue)
        {
        }

        #endregion Constructors

        #region IConfigFactory<IBatchSubscriptionConfig> Members

        public new IBatchSubscriptionConfig GetConfig()
        {
            return new BatchConverter().Convert(this);
        }

        #endregion IConfigFactory<IBatchSubscriptionConfig> Members

        #region IStaticBatchSubscriptionConfig Members

        public double BufferTimeout { get; set; }

        public int BufferSize { get; set; }

        #endregion IStaticBatchSubscriptionConfig Members
    }
}