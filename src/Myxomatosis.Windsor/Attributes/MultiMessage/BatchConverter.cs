using Myxomatosis.Connection.Message;
using Myxomatosis.Windsor.Attributes.SingleMessage;
using System;

namespace Myxomatosis.Windsor.Attributes.MultiMessage
{
    public class BatchConverter : IConverter<IStaticBatchSubscriptionConfig, IBatchSubscriptionConfig>
    {
        #region IConverter<IStaticBatchSubscriptionConfig,IBatchSubscriptionConfig> Members

        public IBatchSubscriptionConfig Convert(IStaticBatchSubscriptionConfig source)
        {
            return new DefaultBatchConfig(source.QueueName)
            {
                CloseTimeout = TimeSpan.FromMilliseconds(source.CloseTimeout),
                OpenTimeout = TimeSpan.FromMilliseconds(source.OpenTimeout),
                SubscriptionId = source.SubscriptionId,
                Name = source.Name,
                BufferSize = source.BufferSize,
                BufferTimeout = TimeSpan.FromMilliseconds(source.BufferTimeout)
            };
        }

        #endregion IConverter<IStaticBatchSubscriptionConfig,IBatchSubscriptionConfig> Members
    }
}