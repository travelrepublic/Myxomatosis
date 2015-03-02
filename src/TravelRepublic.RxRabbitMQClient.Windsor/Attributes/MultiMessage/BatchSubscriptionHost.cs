using System;
using System.Collections.Generic;
using Castle.Core.Logging;
using TravelRepublic.RxRabbitMQClient.Connection;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
{
    internal class BatchSubscriptionHost<T> : ISubscription
    {
        #region Constructors

        public BatchSubscriptionHost(
            IObservableConnection connection,
            IRabbitMessageHandler<IEnumerable<T>> handler,
            IBatchSubscriptionConfig config, ILogger logger
            //            ,
            //            IEnumerable<IBatchRabbitMessageInterceptor<T>> interceptors
            )
        {
            logger.InfoFormat("Subscribing handler: {0} with Id {1}", config.Name, config.Id);
            Subscription = connection.SubscribeOnQueueToMessage(config, handler);
        }

        #endregion Constructors

        public IDisposable Subscription { get; set; }
    }
}