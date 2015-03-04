using Castle.Core.Logging;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using System;
using System.Collections.Generic;

namespace Myxomatosis.Windsor.Attributes.MultiMessage
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

        #region ISubscription Members

        public IDisposable Subscription { get; set; }

        #endregion ISubscription Members
    }
}