using System;
using Castle.Core.Logging;
using TravelRepublic.RxRabbitMQClient.Connection;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
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