using Castle.Core.Logging;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using System;

namespace Myxomatosis.Windsor.Attributes.SingleMessage
{
    internal class SubscriptionHost<T> : ISubscription
    {
        public SubscriptionHost(
            IObservableConnection connection,
            IRabbitMessageHandler<T> handler,
            ISubscriptionConfig config,
            ILogger logger
            //            ,
            //            IEnumerable<IRabbitMessageInterceptor<T>> interceptors
            )
        {
            logger.InfoFormat("Subscribing handler: {0} with Id {1}", config.Name, config.Id);
            Subscription = connection.SubscribeOnQueueToMessage(config, handler);
        }

        public IDisposable Subscription { get; set; }
    }
}