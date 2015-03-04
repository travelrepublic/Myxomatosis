using Myxomatosis.Connection.Message;
using System;

namespace Myxomatosis.Windsor.Attributes.SingleMessage
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class RabbitMessageHandlerAttribute : Attribute, IStaticSubscriptionConfig, IConfigFactory<ISubscriptionConfig>
    {
        #region Constructors

        public RabbitMessageHandlerAttribute(string exchange, string subscriberQueue)
        {
            Exchange = exchange;
            QueueName = subscriberQueue;
        }

        #endregion Constructors

        #region IConfigFactory<ISubscriptionConfig> Members

        public virtual ISubscriptionConfig GetConfig()
        {
            return new SubscriptionConverter().Convert(this);
        }

        #endregion IConfigFactory<ISubscriptionConfig> Members

        #region IStaticSubscriptionConfig Members

        public double Interval { get; set; }

        public string QueueName { get; set; }

        public string Exchange { get; set; }

        public string SubscriptionId { get; set; }

        public double OpenTimeout { get; set; }

        public double CloseTimeout { get; set; }

        public string Name { get; set; }

        #endregion IStaticSubscriptionConfig Members
    }
}