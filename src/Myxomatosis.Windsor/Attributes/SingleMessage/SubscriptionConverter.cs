using Myxomatosis.Connection.Message;
using System;

namespace Myxomatosis.Windsor.Attributes.SingleMessage
{
    public class SubscriptionConverter : IConverter<IStaticSubscriptionConfig, ISubscriptionConfig>
    {
        #region IConverter<IStaticSubscriptionConfig,ISubscriptionConfig> Members

        public ISubscriptionConfig Convert(IStaticSubscriptionConfig source)
        {
            return new DefaultSubscriptionConfig(source.Exchange, source.QueueName)
            {
                CloseTimeout = TimeSpan.FromMilliseconds(source.CloseTimeout),
                OpenTimeout = TimeSpan.FromMilliseconds(source.OpenTimeout),
                Interval = TimeSpan.FromMilliseconds(source.Interval),
                SubscriptionId = source.SubscriptionId,
                Name = source.Name
            };
        }

        #endregion IConverter<IStaticSubscriptionConfig,ISubscriptionConfig> Members
    }
}