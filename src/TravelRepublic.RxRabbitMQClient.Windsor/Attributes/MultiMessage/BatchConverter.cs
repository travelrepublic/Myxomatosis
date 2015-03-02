using System;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
{
    public class BatchConverter : IConverter<IStaticBatchSubscriptionConfig, IBatchSubscriptionConfig>
    {
        #region IConverter<IStaticBatchSubscriptionConfig,IBatchSubscriptionConfig> Members

        public IBatchSubscriptionConfig Convert(IStaticBatchSubscriptionConfig source)
        {
            return new DefaultBatchConfig(source.Exchange, source.QueueName)
            {
                CloseTimeout = TimeSpan.FromMilliseconds(source.CloseTimeout),
                OpenTimeout = TimeSpan.FromMilliseconds(source.OpenTimeout),
                Interval = TimeSpan.FromMilliseconds(source.Interval),
                SubscriptionId = source.SubscriptionId,
                Name = source.Name,
                BufferSize = source.BufferSize,
                BufferTimeout = TimeSpan.FromMilliseconds(source.BufferTimeout)
            };
        }

        #endregion IConverter<IStaticBatchSubscriptionConfig,IBatchSubscriptionConfig> Members
    }
}